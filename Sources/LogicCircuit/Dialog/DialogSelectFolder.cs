using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace LogicCircuit {
#pragma warning disable VSSpell001 // Spell Check
	// This is striped off version of folder select dialog from Windows API Code Pack.
	internal sealed class DialogSelectFolder {
		private enum FileOpenOptions {
			OverwritePrompt = 0x00000002,
			StrictFileTypes = 0x00000004,
			NoChangeDirectory = 0x00000008,
			PickFolders = 0x00000020,

			// Ensure that items returned are filesystem items.
			ForceFilesystem = 0x00000040,

			// Allow choosing items that have no storage.
			AllNonStorageItems = 0x00000080,

			NoValidate = 0x00000100,
			AllowMultiSelect = 0x00000200,
			PathMustExist = 0x00000800,
			FileMustExist = 0x00001000,
			CreatePrompt = 0x00002000,
			ShareAware = 0x00004000,
			NoReadOnlyReturn = 0x00008000,
			NoTestFileCreate = 0x00010000,
			HideMruPlaces = 0x00020000,
			HidePinnedPlaces = 0x00040000,
			NoDereferenceLinks = 0x00100000,
			DontAddToRecent = 0x02000000,
			ForceShowHidden = 0x10000000,
			DefaultNoMiniMode = 0x20000000
		}

		private enum HResult {
			/// <summary>S_OK</summary>
			Ok = 0x0000,

			/// <summary>S_FALSE</summary>
			False = 0x0001,

			/// <summary>E_INVALIDARG</summary>
			InvalidArguments = unchecked((int)0x80070057),

			/// <summary>E_OUTOFMEMORY</summary>
			OutOfMemory = unchecked((int)0x8007000E),

			/// <summary>E_NOINTERFACE</summary>
			NoInterface = unchecked((int)0x80004002),

			/// <summary>E_FAIL</summary>
			Fail = unchecked((int)0x80004005),

			/// <summary>E_ELEMENTNOTFOUND</summary>
			ElementNotFound = unchecked((int)0x80070490),

			/// <summary>TYPE_E_ELEMENTNOTFOUND</summary>
			TypeElementNotFound = unchecked((int)0x8002802B),

			/// <summary>NO_OBJECT</summary>
			NoObject = unchecked((int)0x800401E5),

			/// <summary>Win32 Error code: ERROR_CANCELLED</summary>
			Win32ErrorCanceled = 1223,

			/// <summary>ERROR_CANCELLED</summary>
			Canceled = unchecked((int)0x800704C7),

			/// <summary>The requested resource is in use</summary>
			ResourceInUse = unchecked((int)0x800700AA),

			/// <summary>The requested resources is read-only.</summary>
			AccessDenied = unchecked((int)0x80030005)
		}

		private static class NativeMethods {
			internal static class ShellIIDGuid {
				internal const string FileOpenDialog = "DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7";
				internal const string IFileOpenDialog = "D57C7288-D4AD-4768-BE02-9D969532D960";
				internal const string IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
			}

			internal enum FileDialogAddPlacement { }

			internal enum ShellItemDesignNameOptions {
				Normal = 0x00000000,           // SIGDN_NORMAL
				ParentRelativeParsing = unchecked((int)0x80018001),   // SIGDN_INFOLDER | SIGDN_FORPARSING
				DesktopAbsoluteParsing = unchecked((int)0x80028000),  // SIGDN_FORPARSING
				ParentRelativeEditing = unchecked((int)0x80031001),   // SIGDN_INFOLDER | SIGDN_FOREDITING
				DesktopAbsoluteEditing = unchecked((int)0x8004c000),  // SIGDN_FORPARSING | SIGDN_FORADDRESSBAR
				FileSystemPath = unchecked((int)0x80058000),             // SIGDN_FORPARSING
				Url = unchecked((int)0x80068000),                     // SIGDN_FORPARSING
				ParentRelativeForAddressBar = unchecked((int)0x8007c001),     // SIGDN_INFOLDER | SIGDN_FORPARSING | SIGDN_FORADDRESSBAR
				ParentRelative = unchecked((int)0x80080001)           // SIGDN_INFOLDER
			}

			[Flags]
			internal enum ShellFileGetAttributesOptions { }
			internal enum SICHINTF { }
			internal struct FilterSpec { }
			internal interface IFileDialogEvents { }
			internal interface IShellItemArray { }
			internal interface IShellFolder { }

			// .NET classes representing runtime callable wrappers.
			[ComImport, ClassInterface(ClassInterfaceType.None), TypeLibType(TypeLibTypeFlags.FCanCreate), Guid(ShellIIDGuid.FileOpenDialog)]
			internal class FileOpenDialogRCW { }

			[ComImport, Guid(ShellIIDGuid.IFileOpenDialog), CoClass(typeof(FileOpenDialogRCW))]
			internal interface NativeFileOpenDialog : IFileOpenDialog { }

			[ComImport, Guid(ShellIIDGuid.IShellItem), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			internal interface IShellItem {
				// Not supported: IBindCtx.
				[PreserveSig]
				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				HResult BindToHandler(
					[In] IntPtr pbc,
					[In] ref Guid bhid,
					[In] ref Guid riid,
					[Out, MarshalAs(UnmanagedType.Interface)] out IShellFolder ppv
				);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

				[PreserveSig]
				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				HResult GetDisplayName([In] ShellItemDesignNameOptions sigdnName, out IntPtr ppszName);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetAttributes([In] ShellFileGetAttributesOptions sfgaoMask, out ShellFileGetAttributesOptions psfgaoAttribs);

				[PreserveSig]
				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				HResult Compare(
					[In, MarshalAs(UnmanagedType.Interface)] IShellItem psi,
					[In] SICHINTF hint,
					out int piOrder
				);
			}

			[ComImport(), Guid(ShellIIDGuid.IFileOpenDialog), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			internal interface IFileOpenDialog {
				// Defined on IModalWindow - repeated here due to requirements of COM interop layer.
				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), PreserveSig]
				int Show([In] IntPtr parent);

				// Defined on IFileDialog - repeated here due to requirements of COM interop layer.
				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetFileTypes([In] uint cFileTypes, [In] ref FilterSpec rgFilterSpec);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetFileTypeIndex([In] uint iFileType);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetFileTypeIndex(out uint piFileType);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void Advise([In, MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void Unadvise([In] uint dwCookie);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetOptions([In] FileOpenOptions fos);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetOptions(out FileOpenOptions pfos);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, FileDialogAddPlacement fdap);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void Close([MarshalAs(UnmanagedType.Error)] int hr);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetClientGuid([In] ref Guid guid);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void ClearClientData();

				// Not supported: IShellItemFilter is not defined, converting to IntPtr.
				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);

				// Defined by IFileOpenDialog.
				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetResults([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppenum);

				[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
				void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppsai);
			}

			[DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
			[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
			internal static extern HResult SHCreateItemFromParsingName(
				[MarshalAs(UnmanagedType.LPWStr)] string path,
				// The following parameter is not used - binding context.
				IntPtr pbc,
				ref Guid riid,
				[MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem
			);
		}

		private const FileOpenOptions openOptions = (
			FileOpenOptions.NoTestFileCreate
			| FileOpenOptions.ForceFilesystem
			| FileOpenOptions.PickFolders
			| FileOpenOptions.PathMustExist
		);

		private static string? GetFileNameFromShellItem(NativeMethods.IShellItem item) {
			string? filename = null;
			IntPtr pszString = IntPtr.Zero;
			HResult hr = item.GetDisplayName(NativeMethods.ShellItemDesignNameOptions.DesktopAbsoluteParsing, out pszString);
			if(hr == HResult.Ok && pszString != IntPtr.Zero) {
				filename = Marshal.PtrToStringAuto(pszString);
				Marshal.FreeCoTaskMem(pszString);
			}
			return filename;
		}

		public string? Title { get; set; }
		public string? FileName { get; set; }

		public bool ShowDialog(Window window) {
			ArgumentNullException.ThrowIfNull(window);
			return this.ShowDialog(new WindowInteropHelper(window).Handle);
		}

		[SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
		private bool ShowDialog(IntPtr parent) {
			NativeMethods.NativeFileOpenDialog? dialog = null;
			try {
				dialog = new NativeMethods.NativeFileOpenDialog();
				dialog.SetOptions(DialogSelectFolder.openOptions);
				if(!string.IsNullOrWhiteSpace(this.Title)) {
					dialog.SetTitle(this.Title);
				}
				if(!string.IsNullOrWhiteSpace(this.FileName)) {
					Guid guid = new Guid(NativeMethods.ShellIIDGuid.IShellItem);
					NativeMethods.IShellItem? item = null;
					HResult hr = NativeMethods.SHCreateItemFromParsingName(this.FileName, IntPtr.Zero, ref guid, out item);
					if(hr == HResult.Ok && item != null) {
						dialog.SetFolder(item);
						Marshal.ReleaseComObject(item);
					}
				}
				int result = dialog.Show(parent);
				if(result != (int)HResult.Canceled) {
					NativeMethods.IShellItem? item = null;
					dialog.GetResult(out item);
					if(item != null) {
						this.FileName = DialogSelectFolder.GetFileNameFromShellItem(item);
						Marshal.ReleaseComObject(item);
						return true;
					}
				}
			} finally {
				if(dialog != null) {
					Marshal.ReleaseComObject(dialog);
				}
			}
			return false;
		}
	}
#pragma warning restore VSSpell001 // Spell Check
}
