function setupApp(path) {
  window.easyRpc = {
    url: path,
    templates: {},
    id: 1,
    headers: [{ key: "Content-Type", value: "application/json" }, { key: "", value: "" }],
    activities: []
  };

  ajax(path + 'interface-definition',
    {},
    processJsonResponse);

  fetchTemplate('methods', setupData);
  fetchTemplate('method-info', setupData);
  fetchTemplate('activity-log', attachActivityLog);
  fetchTemplate('header-editor', function (template) { });// pre-load template

  rivets.formatters.uniqueId = function (uniqueId, prefix) {
    return prefix + uniqueId;
  };
}

function attachActivityLog(template) {
  var templateInstance = u(template);
  rivets.bind(templateInstance.nodes[0], easyRpc);
  u('#activityLogDiv').append(templateInstance);
}

function processJsonResponse(error, data) {
  if (error === null) {
    endpointClass(data);
    window.easyRpc.data = data;
    window.easyRpc.setup = true;
    setupData('');
  }
}

function setupData(template) {
  var methodTemplate = window.easyRpc.templates['methods'];
  if (window.easyRpc.data !== null &&
    methodTemplate !== undefined &&
    window.easyRpc.templates['method-info'] !== undefined &&
    window.easyRpc.setup === true) {
    window.easyRpc.setup = false;
    var templateInstance = u(methodTemplate);
    rivets.bind(templateInstance.nodes[0], window.easyRpc.data);
    u('#endpointNavigationPanel').append(templateInstance);
    u('#endpointNavigationPanel label.c-hand').nodes[0].click();
    u('#endpointNavigationPanel a.methodLink').nodes[0].click();
  }
}

function fetchTemplate(name, templateFinished) {
  var template = easyRpc.templates[name];
  if (template === undefined) {
    ajax(window.easyRpc.url + 'templates/' + name + '.html',
      {},
      function (error, data) {
        if (data !== null) {
          window.easyRpc.templates[name] = data;
        }
        templateFinished(data);
      });
  } else {
    templateFinished(template);
  }
}

function endpointClass(endpointList) {
  var arrayLength = endpointList.endpoints.length;
  for (var i = 0; i < arrayLength; i++) {
    var endpoint = endpointList.endpoints[i];
    endpoint.expanded = false;
    var endpointMethodLength = endpoint.Methods.length;
    for (var j = 0; j < endpointMethodLength; j++) {
      var method = endpoint.Methods[j];
      method.endpoint = endpoint;
      method.activate = activateMethod;
      method.executeMethod = executeMethod;
      method.models = associatedModels;
      method.hasModels = hasModels;
      method.tabActivatedHandler = tabActivatedHandler;
      method.displayText = function () {
        if (this.Comments !== null) {
          return this.Comments + '\n' + this.Signature;
        }
        return this.Signature;
      }
      var parameterLength = method.Parameters.length;
      for (var k = 0; k < parameterLength; k++) {
        var parameter = method.Parameters[k];
        parameter.parameterNameClass = parameterNameClass;
        parameter.defaultDisplay = defaultDisplay;
        parameter.currentValue = "";
      }
    }
  }
  processTypeDefinitions(endpointList.dataTypes);
}

function tabActivatedHandler(event, binding) {
  var tabType = u(event.target).data('tab-type');
  var activeTabType = u('#parametersTabDiv li.active a').data('tab-type');
  if (tabType === 'raw') {
    setupRawTextArea(binding);
    u('#parameterDiv').addClass('hide-element');
    u('#headersDiv').addClass('hide-element');
    u('#rawMessageDiv').removeClass('hide-element');
    u('#rawMessageArea').nodes[0].focus();
  } else if (tabType === 'headers') {
    if (activeTabType === 'raw') {
      saveDataToParameters(event, binding);
    }
    setupHeaders();
    u('#parameterDiv').addClass('hide-element');
    u('#rawMessageDiv').addClass('hide-element');
    u('#headersDiv').removeClass('hide-element');
    var headerNodes = u('#headerTable .parameter-input').nodes;
    if (headerNodes.length >= 2) {
      headerNodes[headerNodes.length - 2].focus();
    }
  } else if (tabType === 'parameters') {
    if (activeTabType === 'raw') {
      saveDataToParameters(event, binding);
    }
    u('#rawMessageDiv').addClass('hide-element');
    u('#headersDiv').addClass('hide-element');
    u('#parameterDiv').removeClass('hide-element');
    var paramNodes = u('#parameterTable .parameter-input').nodes;
    if (paramNodes.length > 0) {
      paramNodes[0].focus();
    }
  }
  u('li.tab-item').removeClass('active');
  u(event.target).closest('li').addClass('active');
}

function setupHeaders() {
  fetchTemplate('header-editor',
    function (template) {
      var templateInstance = u(template);
      rivets.bind(templateInstance.nodes[0], easyRpc);
      u('#headersDiv').empty().append(templateInstance);
      u('#addHeader').on('click', function () {
        easyRpc.headers.push({ key: "", value: "" });
      });
      u('#headerTable').on('click', '.header-delete-button', function () {
        var index = Number(u(this).data('header-index'));
        if (index !== 0) {
          easyRpc.headers.splice(index, 1);
        }
      });
    });
}

function saveDataToParameters(event, binding) {
  var stringValues = u('#rawMessageArea').nodes[0].value;
  var arrayValue = JSON.parse(stringValues);
  var arrayValueLength = arrayValue.length;
  var parameterLength = easyRpc.currentMethod.Parameters.length;
  for (var i = 0; i < arrayValueLength && i < parameterLength; i++) {
    easyRpc.currentMethod.Parameters[i].currentValue = JSON.stringify(arrayValue[i]);
  }
}

function setupRawTextArea(binding) {
  var parameterLength = binding.Parameters.length;
  var parameterArray = [];
  for (var k = 0; k < parameterLength; k++) {
    var currentValue = binding.Parameters[k].currentValue;
    if (currentValue === '') {
      parameterArray.push(currentValue);
    } else {
      if (binding.Parameters[k].Stringify === true) {
        parameterArray.push(JSON.parse(currentValue));
      } else {
        parameterArray.push(currentValue);
      }
    }
  }
  u('#rawMessageArea').nodes[0].value = JSON.stringify(parameterArray, undefined, 2);
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
  var activeTab = u('#parametersTabDiv li.active a').data('tab-type');
  var callArrayString = [];
  if (activeTab === 'raw') {
    callArrayString = JSON.parse(u('#rawMessageArea').nodes[0].value);
  } else {
    var inputElements = u('#parameterTable .parameter-input');

    inputElements.each(function (element) {
      if (element.value !== "") {
        if (u(element).data('stringify') === "true") {
          callArrayString.push(JSON.parse(element.value));
        } else {
          callArrayString.push(element.value);
        }
      }
    });
  }

  var headers = {};

  var headerArrayLength = easyRpc.headers.length;
  for (var i = 0; i < headerArrayLength; i++) {
    var entry = easyRpc.headers[i];
    if (entry.key !== "") {
      headers[entry.key] = entry.value;
    }
  }

  var message = { 'jsonrpc': '2.0', 'method': binding.Name, 'params': callArrayString, 'id': easyRpc.id++ };
  var path = sourceElement.data('path');
  var url = window.easyRpc.url + path;

  var startTime;
  try {
    startTime = performance.now();
  } catch (e) {
    // catching
  }

  ajax(url,
    {
      method: "POST",
      body: message,
      headers: headers
    },
    function (error, data) {
      var timeLabel = '';
      try {
        var endTime = performance.now();
        u('#executionTime').nodes[0].textContent = timeLabel = (endTime - startTime).toFixed(1) + ' ms';
      } catch (e) {
        // catch
      }
      if (data !== null && data !== undefined && data.result !== undefined) {
        u('#responseOutput').html('<pre>' + JSON.stringify(data.result, undefined, 2) + '</pre>');
      } else {
        u('#responseOutput').html('<pre>' + JSON.stringify(data !== '' ? data : error, undefined, 2) + '</pre>');
      }
      var status = error === null ? "200 OK" : error;
      u('#httpStatus').nodes[0].textContent = status;
      u('#responseDiv').removeClass('hide-element');
      easyRpc.activities.unshift(activityRecord(binding, path, message, error, data, timeLabel));
      if (easyRpc.activities.length > 15) {
        easyRpc.activities.pop();
      }
    });
}

function activityRecord(method, path, message, error, data, timeLabel) {
  var statusMessage;
  var alertClass;
  if (error !== null) {
    statusMessage = error;
    alertClass = "alert alert-danger clickable";
  } else if (data.result === undefined) {
    statusMessage = "error";
    alertClass = "alert alert-danger clickable";
  } else {
    statusMessage = "success " + timeLabel;
    alertClass = "alert alert-success clickable";
  }
  var parameters = JSON.stringify(message.params);
  if (parameters.length > 20) {
    parameters = parameters.substring(1, 20) + '...';
  } else {
    parameters = parameters.substring(1, parameters.length - 1);
  }
  parameters = '(' + parameters + ')';

  return {
    path: path,
    message: message,
    status: statusMessage,
    alertClass: alertClass,
    method: method,
    parameters: parameters,
    clickHandler: function () {
      debugger;
      method.endpoint.expanded = true;
      activateMethodTemplate(method);
      for (var i = 0; i < method.Parameters.length; i++) {
        method.Parameters[i].currentValue = message.params[i];
      }
    }
  };
}


function activateMethod(event, binding) {
  debugger;
  activateMethodTemplate(binding.method);
}

function activateMethodTemplate(methodData) {
  easyRpc.currentMethod = methodData;
  var template = window.easyRpc.templates["method-info"];
  var templateInstance = u(template);
  rivets.bind(templateInstance.nodes[0], methodData);
  u('#mainContentContainer').empty().append(templateInstance);
  var paramNodes = u('#parameterTable .parameter-input').nodes;
  if (paramNodes.length > 0) {
    paramNodes[0].focus();
  }
}

function activateType(event, binding) {
  fetchTemplate('type-definitions-list', function (template) {
    var templateInstance = u(template);
    rivets.bind(templateInstance.nodes[0], easyRpc.data);
    u('#mainContentContainer').empty().append(templateInstance);
  });
}