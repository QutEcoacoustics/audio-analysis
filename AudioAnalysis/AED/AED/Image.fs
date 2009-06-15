#light

module Image

open Microsoft.FSharp.NativeInterop
open System.Drawing 
open System.Drawing.Imaging

let invert img = Array2.map (fun x -> 255s - x) img
     
(* Copied and modified from "Greyscale Image Processing in F#" by Chance Coble
   http://leibnizdream.wordpress.com/2008/01/30/grayscale-image-processing-in-f/
 *)
let fromBitmap (img: Bitmap) =
     // TODO what should the type of this array be?
     let (a:int16[,]) = Array2.create img.Height img.Width (int16 0)
     let bd = img.LockBits(Rectangle(0,0,img.Width,img.Height),ImageLockMode.ReadWrite,PixelFormat.Format8bppIndexed)  
     let mutable (p:nativeptr<byte>) = NativePtr.of_nativeint (bd.Scan0)
     for i=0 to img.Height-1 do  
      for j=0 to img.Width-1 do   
       a.[i,j] <- (int16 (NativePtr.get p 0))  
       p <- NativePtr.add p 1  
      done   
      p <- NativePtr.add p (bd.Stride - bd.Width)  
     done  
     img.UnlockBits(bd)  
     a
     
let private setPosition p i j v =  
     NativePtr.set p 0 (byte v)  
     NativePtr.set p 1 (byte v)  
     NativePtr.set p 2 (byte v) 
     
let toBitmap (arr:int16[,]) =  
    let image = new Bitmap(arr.GetLength(0),arr.GetLength(1))  
    let bd = image.LockBits(Rectangle(0,0,image.Width,image.Height),ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb)  
    let mutable (p:nativeptr<byte>) = NativePtr.of_nativeint (bd.Scan0)  
    for i=0 to image.Width-1 do  
      for j=0 to image.Height-1 do  
        setPosition p i j (arr.[i,j])  
        p <- NativePtr.add p 4  
      done  
      p <- NativePtr.add p (bd.Stride - bd.Width*4)  
    done  
    image.UnlockBits(bd)  
    image 
     
// End copy
     
let fromFile(s:string) = fromBitmap(new Bitmap(s))

let toFile img (s:string) =
     let b = toBitmap(img)
     b.Save(s, System.Drawing.Imaging.ImageFormat.Jpeg)