namespace Common.Core;

[Flags]
public enum EMode
{ 
    Host   = 0x10,
    Client = 0x01,
    Both   = Host | Client
}