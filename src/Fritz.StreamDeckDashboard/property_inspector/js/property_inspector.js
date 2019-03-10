// global websocket, used to communicate from/to Stream Deck software
// as well as some info about our plugin, as sent by Stream Deck software 
var websocket = null,
    uuid = null,
    inInfo = null,
    actionInfo = {},
    onmessage = null,
    settingsModel = {
        ProjectFileName: ''
    };

function connectElgatoStreamDeckSocket(inPort, inUUID, inRegisterEvent, inInfo, inActionInfo) {
    uuid = inUUID;
    actionInfo = JSON.parse(inActionInfo);
    inInfo = JSON.parse(inInfo);
    websocket = new WebSocket('ws://localhost:' + inPort);

    if (actionInfo.payload.settings.settingsModel) {
        receivedSettings(actionInfo);
    }

    websocket.onopen = function () {

        if (onmessage) websocket.onmessage = onmessage;

        var json = { event: inRegisterEvent, uuid: inUUID };
        // register property inspector to Stream Deck
        websocket.send(JSON.stringify(json));
    };

    websocket.onmessage = function (evt) {
        var jsonObj = JSON.parse(evt.data);
        var sdEvent = jsonObj['event'];
        switch (sdEvent) {
            case 'didReceiveSettings':
                receivedSettings(jsonObj);
                break;
            default:
                break;
        }
    };

    function receivedSettings(jsonObj) {

        if (jsonObj.payload.settings.settingsModel.ProjectFileName) {
            var fileLabel = document.getElementById("fileLabel");
            var name = jsonObj.payload.settings.settingsModel.ProjectFileName;
            settingsModel.ProjectFileName = name;
            fileLabel.innerText = name.replace(/^.*[\\\/]/, '');
        }
    }
}

function sendValueToPlugin(value, param) {
    if (websocket) {
        settingsModel[param] = value;
        const json = {
            "event": "setSettings",
            "context": uuid,
            "payload": {
                "settingsModel": settingsModel
            }
        };
        websocket.send(JSON.stringify(json));
    }
}


