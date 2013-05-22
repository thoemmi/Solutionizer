using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Solutionizer.Helper {
    public static class Icons {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int ExtractIconEx(string szFileName, int nIconIndex,
                                                IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);

        private static Icon ExtractIconFromExe(string file, int iconIndex, bool large) {
            var hDummy = new[] {
                IntPtr.Zero
            };
            var hIconEx = new[] {
                IntPtr.Zero
            };

            try {
                var readIconCount = large
                                        ? ExtractIconEx(file, iconIndex, hIconEx, hDummy, 1)
                                        : ExtractIconEx(file, iconIndex, hDummy, hIconEx, 1);

                if (readIconCount > 0 && hIconEx[0] != IntPtr.Zero) {
                    return (Icon) Icon.FromHandle(hIconEx[0]).Clone();
                } else {
                    return null;
                }
            } finally {
                foreach (var ptr in hIconEx.Where(ptr => ptr != IntPtr.Zero)) {
                    DestroyIcon(ptr);
                }

                foreach (var ptr in hDummy.Where(ptr => ptr != IntPtr.Zero)) {
                    DestroyIcon(ptr);
                }
            }
        }

        private static string GetFileAndIconIndex(string extension) {
            using (var rkFileType = Registry.ClassesRoot.OpenSubKey(extension)) {
                if (rkFileType != null) {
                    var defaultValue = rkFileType.GetValue("") as string;
                    if (defaultValue != null) {
                        var defaultIcon = defaultValue + "\\DefaultIcon";
                        using (var rkFileIcon = Registry.ClassesRoot.OpenSubKey(defaultIcon)) {
                            if (rkFileIcon != null) {
                                var value = rkFileIcon.GetValue("") as string;
                                if (value != null) {
                                    return value.Replace("\"", "");
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static readonly ConcurrentDictionary<string, ImageSource> _imagesForExtensions = new ConcurrentDictionary<string, ImageSource>();
        private static readonly ConcurrentDictionary<string, ImageSource> _imagesForFileAndIndex = new ConcurrentDictionary<string, ImageSource>();

        public static ImageSource GetImageFromFileAndIndex(string file, int iconIndex) {
            return _imagesForFileAndIndex.GetOrAdd(file + "-" + iconIndex, _ => {
                var icon = ExtractIconFromExe(file, iconIndex, false);
                if (icon != null) {
                    try {
                        ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        imageSource.Freeze();
                        return imageSource;
                    } finally {
                        icon.Dispose();
                    }
                }
                return null;
            });
        }
        
        public static ImageSource GetImageForFile(string path) {
            var fileextension = Path.GetExtension(path);
            if (String.IsNullOrEmpty(fileextension)) {
                return null;
            }

            return _imagesForExtensions.GetOrAdd(fileextension, extension => {
                var fileAndIndex = GetFileAndIconIndex(extension);
                if (!String.IsNullOrEmpty(fileAndIndex)) {
                    var parts = fileAndIndex.Split(',');
                    var file = parts[0];
                    var iconIndex = parts.Length == 1 ? 0 : Int32.Parse(parts[1]);

                    return GetImageFromFileAndIndex(file, iconIndex);
                }
                return null;
            });
        }
    }
}