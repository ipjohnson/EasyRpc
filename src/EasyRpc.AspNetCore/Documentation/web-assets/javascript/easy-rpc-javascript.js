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
      endpoint.Methods[j].activate = activateMethod;
      endpoint.Methods[j].executeMethod = executeMethod;
    }
  }
}

function executeMethod(event, binding) {
  var panelBody = u(event.srcElement).closest('div.panel-body');
  var inputElements = panelBody.find('.parameter-input');
  var callArrayString = [];
  inputElements.each(function (element) {
    callArrayString.push(element.value);
  });

  var message = { 'jsonrpc': '2.0', 'method': binding.Name, 'params': callArrayString, 'id': 1 };

  ajax(window.easyRpc.url + 'IntMath',
    {
      method: "POST",
      body: message,
      headers: { "Content-Type": "application/json" }
    },
    function (error, data) {
      if (data.result !== undefined) {
        u('#responseOutput').append('<pre>' + JSON.stringify(data.result, undefined, 2) + '</pre>');
      } else {
        u('#responseOutput').append('<pre>' + JSON.stringify(data !== '' ? data : error, undefined, 2) + '</pre>');
      }
    });

  //hljs.highlightBlock(u('#responseOutput').nodes[0]);
}

function activateMethod(event, binding) {
  var add = true;
  var methodData = binding.method;
  u('#tabContent').children().each(function (element) {
    var data = u(element).nodes[0].dataset.methodInfo;
    if (data === methodData) {
      add = false;
      u(element).removeClass('hide-element');
    } else {
      u(element).addClass('hide-element');
    }
  });

  if (add === true) {
    var template = window.easyRpc.templates["method-info"];
    var templateInstance = u(template);

    var tabList = u('#tabList');

    var instanceNumber = tabList.children().length + 1;
    tabList.append('<li class="tab-item"><label for="tab' + instanceNumber + '"><a>' + methodData.Name + '</a></label></li>');
    rivets.bind(templateInstance.nodes[0], methodData);
    templateInstance.nodes[0].dataset.methodInfo = methodData;
    u('#tabContent').append(templateInstance);
  }
}


