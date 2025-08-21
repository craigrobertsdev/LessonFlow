let textEditorRef;
let keyPressEvent;

function preventDefault() {
    if (keyPressEvent !== null) {
        keyPressEvent.preventDefault();
    }
}

function clearKeyPressEvent() {
    if (keyPressEvent !== null) {
        keyPressEvent = null;
    }
}

function init(textEditor) {
    textEditorRef = textEditor;
    console.log("Text editor = " + textEditorRef);
}

document.addEventListener("keydown", async function(event) {
    if (event.key === "Tab" && event.shiftKey) {
        keyPressEvent = event;
        await textEditorRef.invokeMethodAsync("HandleTabOutdent");
        keyPressevent = null;
    }
    else if (event.key === "Tab") {
        keyPressEvent = event;
        await textEditorRef.invokeMethodAsync("HandleTabIndent");
        keyPressevent = null;
    }
});

