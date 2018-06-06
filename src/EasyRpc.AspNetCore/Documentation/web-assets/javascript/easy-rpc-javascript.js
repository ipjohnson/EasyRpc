function setupApp(path) {
  window.easyRpc = { url: path, templates: {} };

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
  ajax(window.easyRpc.url + 'templates/' + name + '.html',
    {},
    function (error, data) {
      if (data !== null) {
        window.easyRpc.templates[name] = data;
      }
      templateFinished();
    });
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
      method.displayText = function () {
        if (this.Comments !== null) {
          return this.Comments + '\n' + this.Signature;
        }
        return this.Signature;
      }
    }
  }
}

function executeMethod(event, binding) {
  var sourceElement = u(event.target);
  var panelBody = sourceElement.closest('div.panel-body');
  var inputElements = panelBody.find('.parameter-input');
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

  var message = { 'jsonrpc': '2.0', 'method': binding.Name, 'params': callArrayString, 'id': 1 };
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


