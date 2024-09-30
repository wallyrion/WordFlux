export function init(id, group, pull, put, sort, handle, filter, component, forceFallback) {
    var sortable = new Sortable(document.getElementById(id), {
        animation: 200,
        group: {
            name: group,
            pull: pull || true,
            put: put
        },
        filter: filter || undefined,
        sort: sort,
        forceFallback: forceFallback,
        handle: handle || undefined,
        onChoose: function (/**Event*/evt) {
            console.log("on choose")
            evt.oldIndex;  // element index within parent
        },
        onStart: function (/**Event*/evt) {
            console.log("on start drag")
            window.getSelection().removeAllRanges();

        },
        onEnd: function (/**Event*/evt) {
            console.log("on end drag")
            window.getSelection().removeAllRanges();


        },
        
        onUpdate: (event) => {
            // Revert the DOM to match the .NET state
            event.item.remove();
            event.to.insertBefore(event.item, event.to.childNodes[event.oldIndex]);

            // Notify .NET to update its model and re-render
            component.invokeMethodAsync('OnUpdateJS', event.oldDraggableIndex, event.newDraggableIndex);
        },
        
        onRemove: (event) => {
            if (event.pullMode === 'clone') {
                // Remove the clone
                event.clone.remove();
            }

            event.item.remove();
            event.from.insertBefore(event.item, event.from.childNodes[event.oldIndex]);

            // Notify .NET to update its model and re-render
            component.invokeMethodAsync('OnRemoveJS', event.oldDraggableIndex, event.newDraggableIndex);
        }
    });
}