mergeInto(LibraryManager.library,
{
    /// <summary>
    /// 显式保留文件系统、IDBFS、UTF8 回调字符串所需的依赖
    /// </summary>
    SaveData_SynchronizeFiles__deps: ['$FS', '$IDBFS', '$UTF8ToString', '$stringToNewUTF8', 'free'],

    /// <summary>
    /// 提交持久文件系统并以字符串请求标识通知托管调用方
    /// </summary>
    SaveData_SynchronizeFiles: function (directory, requestId, callback)
    {
        var path = UTF8ToString(directory);
        var id = UTF8ToString(requestId);
        var completed = false;

        /// <summary>
        /// 每个请求只通知一次，托管回调复制字符串后释放临时原生内存
        /// </summary>
        function complete(error)
        {
            if (completed) return;

            completed = true;
            var idPointer = stringToNewUTF8(id);
            
            var errorMessage;
            if (error)
            {
                errorMessage = String(error.message || error);
            }
            else
            {
                errorMessage = '';
            }

            var errorPointer = stringToNewUTF8(errorMessage);

            try
            {
                {{{ makeDynCall('vii', 'callback') }}}(idPointer, errorPointer);
            }
            finally
            {
                _free(idPointer);
                _free(errorPointer);
            }
        }

        try
        {
            var node = FS.lookupPath(path).node;

            if (!node.mount || node.mount.type !== IDBFS)
            {
                throw new Error('SaveData directory is not backed by IDBFS');
            }

            FS.syncfs(false, complete);
        }
        catch (error)
        {
            complete(error);
        }
    }
});
