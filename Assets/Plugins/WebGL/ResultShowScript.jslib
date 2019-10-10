mergeInto(LibraryManager.library, {
 
  OpenTweetWindow: function(text, hashtags, url) {
    window.open('https://twitter.com/intent/tweet?text=' + Pointer_stringify(text) + '&hashtags=' + Pointer_stringify(hashtags) + '&url=' + Pointer_stringify(url));
  }
 
});