﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;

namespace Backup
{
    public class PrivilegeDriver
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        // Use this signature if you do not want the previous state
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AdjustTokenPrivileges(IntPtr tokenHandle,
            [MarshalAs(UnmanagedType.Bool)]bool disableAllPrivileges,
            ref TOKEN_PRIVILEGES newState,
            UInt32 bufferLength,
            IntPtr previousState,
            IntPtr returnLength);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool OpenProcessToken
            (IntPtr processHandle, int desiredAccess, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool LookupPrivilegeValue
                (string host, string name, ref LUID lpLuid);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TOKEN_PRIVILEGES
        {
            public UInt32 PrivilegeCount;
            public LUID Luid;
            public UInt32 Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        const int SE_PRIVILEGE_ENABLED = 0x00000002;
        const int TOKEN_QUERY = 0x00000008;
        const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        const int ERROR_NOT_ALL_ASSIGNED = 1300;
        //http://msdn.microsoft.com/en-us/library/bb530716(VS.85).aspx
        const string SE_RESTORE_PRIVILEGE = "SeRestorePrivilege";
        const string SE_BACKUP_PRIVILEGE = "SeBackupPrivilege";
        const string SE_TAKE_OWNER_SHIP_PRIVILEGE = "SeTakeOwnershipPrivilege";

        public void UpPrivilege()
        {
            GivePrivilege(SE_RESTORE_PRIVILEGE);
            GivePrivilege(SE_BACKUP_PRIVILEGE);
            GivePrivilege(SE_TAKE_OWNER_SHIP_PRIVILEGE);
        }

        private string CurrentUser { get; set; }

        private void GivePrivilege(string privilege)
        {
            TOKEN_PRIVILEGES tokenPrivileges;
            tokenPrivileges.PrivilegeCount = 1;
            tokenPrivileges.Luid = new LUID();
            tokenPrivileges.Attributes = SE_PRIVILEGE_ENABLED;

            IntPtr tokenHandle = RetrieveProcessToken();

            try
            {
                bool success = LookupPrivilegeValue
                            (null, privilege, ref tokenPrivileges.Luid);

                if (success == false)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    throw new Exception(
                        string.Format("Could not find privilege {0}. Error {1}",
                                            privilege, lastError));
                }

                success = AdjustTokenPrivileges(
                                                    tokenHandle, false,
                                                    ref tokenPrivileges, 0,
                                                    IntPtr.Zero, IntPtr.Zero);
                if (success == false)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    throw new Exception(
                        string.Format("Could not assign privilege {0}. Error {1}",
                                        privilege, lastError));
                }
            }
            finally
            {
                CloseHandle(tokenHandle);
            }

        }

        private IntPtr RetrieveProcessToken()
        {
            IntPtr processHandle = GetCurrentProcess();
            IntPtr tokenHandle = IntPtr.Zero;
            bool success = OpenProcessToken(processHandle,
                                            TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY,
                                            ref tokenHandle);
            if (success == false)
            {
                int lastError = Marshal.GetLastWin32Error();
                throw new Exception(
                    string.Format("Could not retrieve process token. Error {0}",
                                        lastError));
            }
            return tokenHandle;
        }

        private string GetCurrentUser()
        {
            var user = WindowsIdentity.GetCurrent();
            return user.Name;
        }
        private string GetOwnerFile(string path)
        {
            var owner = File.GetAccessControl(path).GetOwner(typeof(NTAccount));
            return owner.ToString();
        }

    }
}