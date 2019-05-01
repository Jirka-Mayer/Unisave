using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Callbacks;
using UnityEditor.Build.Reporting;
using System.Linq;

namespace Unisave.CodeUploader
{
    public static class CompilationFinishedHook
    {
        [InitializeOnLoadMethod]
        static void OnInitializeOnLoad()
        {
            CompilationPipeline.assemblyCompilationFinished += OnCompilationFinished;
        }

        static void OnCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            // ignore editor assembly build
            if (assemblyPath.Contains("Editor"))
                return;

            // Do nothing if there were compile errors on the target
            if (CompilerMessagesContainError(messages))
            {
                Debug.Log("CodeUploader: stop because compile errors on target");
                return;
            }

            Uploader uploader = Uploader.CreateDefaultInstance();
            uploader.Run();
        }

        static bool CompilerMessagesContainError(CompilerMessage[] messages)
        {
            return messages.Any(msg => msg.type == CompilerMessageType.Error);
        }

        // Testing:
        [PostProcessBuild(1)]
        static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            Debug.Log("CodeUploader: Postprocess build");
            Debug.Log(target);
            Debug.Log(pathToBuiltProject);
            Debug.Log(Application.buildGUID);
        }
    }
}