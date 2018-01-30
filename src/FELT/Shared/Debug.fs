(* 
    http://diditwith.net/2008/12/29/PrintfAndFormattingDebugOutputInF.aspx
*)

namespace Felt.Shared
    module Debug =
      open System.Diagnostics

      let writef fmt = Printf.ksprintf Debug.Write fmt

      let writefn fmt = Printf.ksprintf Debug.WriteLine fmt