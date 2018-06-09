function setupApp(path) {
  window.easyRpc = { url: path, templates: {}, id: 1 };

  ajax(path + 'interface-definition',
    {},
    processJsonResponse);

  fetchTemplate('methods', setupData);
  fetchTemplate('method-info', setupData);

  rivets.formatters.uniqueId = function (uniqueId, prefix) {
    return prefix + uniqueId;
  };
}

function processJsonResponse(error, data) {
  if (error === null) {
    endpointClass(data);
    window.easyRpc.data = data;
    window.easyRpc.setup = true;
    setupData();
  }
}

function setupData() {
  var methodTemplate = window.easyRpc.templates['methods'];
  if (window.easyRpc.data !== null &&
    methodTemplate !== undefined &&
    window.easyRpc.templates['method-info'] !== undefined &&
    window.easyRpc.setup === true) {
    window.easyRpc.setup = false;
    var templateInstance = u(methodTemplate);
    rivets.bind(templateInstance.nodes[0], window.easyRpc.data);
    u('#endpointNavigationPanel').append(templateInstance);
  }
}

function fetchTemplate(name, templateFinished) {
  if (easyRpc.templates[name] === undefined) {
    ajax(window.easyRpc.url + 'templates/' + name + '.html',
      {},
      function(error, data) {
        if (data !== null) {
          window.easyRpc.templates[name] = data;
        }
        templateFinished();
      });
  } else {
    templateFinished();
  }
}

function endpointClass(endpointList) {
  var arrayLength = endpointList.endpoints.length;
  for (var i = 0; i < arrayLength; i++) {
    var endpoint = endpointList.endpoints[i];

    var endpointMethodLength = endpoint.Methods.length;
    for (var j = 0; j < endpointMethodLength; j++) {
      var method = endpoint.Methods[j];
      method.endpoint = endpoint;
      method.activate = activateMethod;
      method.executeMethod = executeMethod;
      method.models = associatedModels;
      method.hasModels = hasModels;
      method.displayText = function () {
        if (this.Comments !== null) {
          return this.Comments + '\n' + this.Signature;
        }
        return this.Signature;
      }
      var parameterLength = method.Parameters.length;
      for (var k = 0; k < parameterLength; k++) {
        method.Parameters[k].parameterNameClass = parameterNameClass;
        method.Parameters[k].defaultDisplay = defaultDisplay;
      }
    }
  }
  processTypeDefinitions(endpointList.dataTypes);
}
function defaultDisplay() {
  if (this.DefaultValue !== undefined && this.DefaultValue !== null) {
    return 'Default: ' + this.DefaultValue;
  }
  return '';
}

function parameterNameClass() {
  if (this.Optional !== true) {
    return 'model-required';
  }
  return '';
}

function hasModels() {
  return this.models().length > 0;
}

function associatedModels() {
 if (this.modelList !== undefined) {
    return this.modelList;
  }
  var models = [];
  getModelsForType(models, this.ReturnType.FullName);
  var arrayLength = this.Parameters.length;
  for (var i = 0; i < arrayLength; i++) {
    getModelsForType(models, this.Parameters[i].ParameterType.FullName);
  }
  this.modelList = models;
  return models;
}

function getModelsForType(models, name) {
  var returnTypeModel = easyRpc.dataTypes[name];
  if (returnTypeModel !== undefined && 
    models.indexOf(returnTypeModel) === -1) {
    models.push(returnTypeModel);
    var arrayLength = returnTypeModel.Properties.length;
    for (var i = 0; i < arrayLength; i++) {
      getModelsForType(models, returnTypeModel.Properties[i].PropertyType.FullName);
    }
  }
}

function processTypeDefinitions(dataTypes) {
  var arrayLength = dataTypes.length;
  var typeDictionary = window.easyRpc.dataTypes = {};
  for (var i = 0; i < arrayLength; i++) {
    var type = dataTypes[i];
    type.activate = activateType;
    type.displayComment = displayComment;
    typeDictionary[type.FullName] = type;
    var propsLength = type.Properties.length;
    for (var j = 0; j < propsLength; j++) {
      type.Properties[j].displayComment = displayComment;
      type.Properties[j].propertyNameClass = propertyNameClass;
    }
  }
}
function propertyNameClass() {
  if (this.Required === true) {
    return 'model-required';
  }
  return false;
}

function displayComment() {
  if (this.Comment !== undefined &&
    this.Comment !== null &&
    this.Comment.length > 0) {
    return ' // ' + this.Comment;
  }
  return '';
}

function executeMethod(event, binding) {
  var sourceElement = u(event.target);
  var inputElements = u('#parameterTable .parameter-input');
  var callArrayString = [];
  inputElements.each(function (element) {
    if (element.value !== "") {
      if (u(element).data('stringify') === "true") {
        callArrayString.push(JSON.parse(element.value));
      } else {
        callArrayString.push(element.value);
      }
    }
  });

  var message = { 'jsonrpc': '2.0', 'method': binding.Name, 'params': callArrayString, 'id': easyRpc.id++ };
  var startTime;
  try {
    startTime = performance.now();
  } catch (e) {
    // catching
  }
  var url = window.easyRpc.url + sourceElement.data('path');

  ajax(url,
    {
      method: "POST",
      body: message,
      headers: { "Content-Type": "application/json" }
    },
    function (error, data) {
      try {
        var endTime = performance.now();
        u('#executionTime').nodes[0].textContent = (endTime - startTime).toFixed(1) + ' ms';
      } catch (e) {
        // catch
      }
      if (data.result !== undefined) {
        u('#responseOutput').html('<pre>' + JSON.stringify(data.result, undefined, 2) + '</pre>');
      } else {
        u('#responseOutput').html('<pre>' + JSON.stringify(data !== '' ? data : error, undefined, 2) + '</pre>');
      }
      var status = error === null ? "200 OK" : error;
      u('#httpStatus').nodes[0].textContent = status;
      u('#responseDiv').removeClass('hide-element');
    });
}

function activateMethod(event, binding) {
  var methodData = binding.method;
  var template = window.easyRpc.templates["method-info"];
  var templateInstance = u(template);
  rivets.bind(templateInstance.nodes[0], methodData);
  templateInstance.nodes[0].dataset.methodInfo = methodData;
  u('#mainContentContainer').empty().append(templateInstance);
}

function activateType(event, binding) {
 fetchTemplate('type-definitions-list', function() {
    var template = window.easyRpc.templates["type-definitions-list"];
    var templateInstance = u(template);
    rivets.bind(templateInstance.nodes[0], easyRpc.data);
    u('#mainContentContainer').empty().append(templateInstance);
  });
}

