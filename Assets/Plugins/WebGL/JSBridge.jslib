mergeInto(LibraryManager.library, {
  NotifyPartSelected: function(partIdPtr) {
    var partId = UTF8ToString(partIdPtr);
    if (window.onUnityPartSelected) {
      window.onUnityPartSelected(partId);
    }
  }
});
