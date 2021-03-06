#if TOOLS
using Godot;
using System.Collections.Generic;

namespace CustomResourceLoaderPlugin
{
    [Tool]
    public class CustomResourceLoader : EditorPlugin
    {
        private List<string> _resourceNames;

        private RegEx _regex;

        private Texture _icon;

        public override void _EnterTree()
        {
            var editorInterface = GetEditorInterface();
            var efs = editorInterface.GetResourceFilesystem();
            efs.Connect("filesystem_changed", this, "OnfilesystemChanged");

            _icon = editorInterface.GetBaseControl().GetIcon("Node", "SpatialMaterial");
            _resourceNames = new List<string>();

            _regex = new RegEx();
            _regex.Compile(@"class\s(.+)\s:\sResource");

            UpdateCustomResources();
        }

        private void UpdateCustomResources()
        {
            var dir = new Godot.Directory();
            foreach (string item in GetAllScripts(dir.GetCurrentDir()))
            {
                // Initialization of the plugin goes here.
                // Add the new type with a name, a parent type, a script and an icon.
                var script = GD.Load<Script>(item);
                var className = GetClassName(script.SourceCode);

                if (!_resourceNames.Contains(className))
                {
                    _resourceNames.Add(className);
                    AddCustomType($"{className} ({item.GetFile()})", "Resource", script, null);
                }
            }
        }

        private void RemoveCustomResources()
        {
            foreach (var name in _resourceNames)
            {
                // Clean-up of the plugin goes here.
                // Always remember to remove it from the engine when deactivated.
                RemoveCustomType(name);
            }
        }

        private string GetClassName(string sourceCode)
        {
            var match = _regex.Search(sourceCode);
            if (match == null)
                return "";

            return match.GetString(1);
        }

        private List<string> GetAllScripts(string path)
        {
            var dirs = new List<string>();
            var dir = new Godot.Directory();
            dir.Open(path);

            if (dir.FileExists(path))
            {
                dirs.Add(path);
                return dirs;
            }

            dir.ListDirBegin(true, true);
            while (true)
            {
                string subpath = dir.GetNext();
                if (subpath.Empty())
                    break;

                if (subpath.BeginsWith(".") || !subpath.EndsWith(".cs"))
                    continue;

                var subDirs = GetAllScripts(path.PlusFile(subpath));

                dirs.AddRange(subDirs);
            }

            return dirs;
        }

        public override void _ExitTree()
        {
            foreach (var name in _resourceNames)
            {
                // Clean-up of the plugin goes here.
                // Always remember to remove it from the engine when deactivated.
                RemoveCustomType(name);
            }
        }

        public void OnfilesystemChanged()
        {
            RemoveCustomResources();
            UpdateCustomResources();
        }
    }
}
#endif
