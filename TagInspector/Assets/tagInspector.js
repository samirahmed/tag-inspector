// require ace internals
var TokenIterator = require("ace/token_iterator").TokenIterator;
var Range = require('ace/range').Range;

// app KnockOut ViewModel
var app = {

    serverTime: ko.observable(),

    createdAt: ko.observable(),

    tags: ko.observableArray(),

    isLoading: ko.observable(false),

    hasLoaded: ko.observable(false),

    statusCode: ko.observable(),

    triggerLoad: function (formElement) {
        editor.setValue("");
        loadHtml($("#uri").val());
    },

    markers: [],

    highlight: function (tagItem) {
        var name = tagItem.tag;
        console.log(name);
        var isFirst = true;

        app.markers.forEach(function (marker) {
            editor.getSession().removeMarker(marker);
        });

        var iterator = new TokenIterator(editor.getSession(), 0, 0);

        do {
            var current = iterator.getCurrentToken();
            if (!current) continue;
            if (current.type.indexOf("tag-name") < 0) continue;
            if (current.value !== name) continue;
            var col = iterator.getCurrentTokenColumn();
            var row = iterator.getCurrentTokenRow();
            var end = iterator.getCurrentToken().value.length;
            var range = new Range(row, col, row, col+end);
            app.markers.push(editor.getSession().addMarker(range, "tag-highlight", "text"));
            if (isFirst) {
                isFirst = false;
                editor.gotoLine(row);
            }
        } while (iterator.stepForward());
    }
};

// displays Html Source
function displayHtml(rawHtml) {
    editor.setValue(rawHtml);
    editor.clearSelection();
    editor.gotoLine(0);
}

// loads HTML Summary from api
function loadHtml(url) {

    app.isLoading(true);
    $.getJSON("api/summary?url=" + encodeURI(url))
        .done(function(data) {
            if (data.failureReason) {
                displayHtml(data.failureReason);
            } else {
                displayHtml(data.body);
                app.hasLoaded(true);
                app.serverTime(Math.round(data.pageLoadTime));
                app.statusCode(data.statusCode);
                app.createdAt(new Date(data.createdAt).toString());
                app.tags.removeAll();
                Object.keys(data.frequency).forEach(function(key) {
                    app.tags.push({ tag: key, count: data.frequency[key] });
                });
            }
        }).fail(function(jqxhr, textStatus, error) {
            var err = textStatus + ", " + error;
            console.log("Request Failed: " + err);
            displayHtml("Request Failed: " + err);
        }).always(function() {
            app.isLoading(false);
        });
}

// configure ace editor
var editor = ace.edit("editor");
editor.setTheme("ace/theme/monokai");
editor.setShowPrintMargin(false);
editor.setReadOnly(true);
editor.getSession().setMode("ace/mode/html");
editor.getSession().setUseWrapMode(true);
displayHtml("");

ko.applyBindings(app);