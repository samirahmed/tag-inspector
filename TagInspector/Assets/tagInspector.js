function displayHtml(rawHtml) {
    var prettyHtml = html_beautify(rawHtml);
    editor.setValue(prettyHtml);
}

function triggerLoad() {
    loadHtml($("#uri").val());
}

function loadHtml(url) {
    $.getJSON("api/summary?url=" + encodeURI(url), function(data) {

        if (data.failureReason) {
            // failure
        }
        else {
            displayHtml(data.body);
        }
    });
}

var editor = ace.edit("editor");
editor.setTheme("ace/theme/monokai");
editor.setShowPrintMargin(false);
editor.setReadOnly(true);
editor.getSession().setMode("ace/mode/html");
displayHtml("<html><body><p>Enter A Url To Inspect</p></body></html>");

