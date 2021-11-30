mergeInto(LibraryManager.library, {

  // KeyQueue を初期化
  InitKeyCodeQueue: function() {
    keyQueue = [];
  },

  // JS での KeyCode 取得
  GetKeyCodeFromJS: function() {
    var returnStr = "None";
    if (keyQueue.length > 0){
      console.log("koyaya");
      returnStr = keyQueue[0];
      keyQueue.shift();
    }
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  }
});