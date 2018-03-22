# FAQ

## Who is this program designed for?

Short answer: computers.

_AP.exe_ is designed primarily to be used by people that have significant computing
expertise. It designed specifically to be used by **other programs** for any real workloads.
It is not designed to be human friendly.

We encourage anyone to give it a go--don't be daunted by these docs--but keep in
mind the target audience. You're in the right ballpark if:

- If your workload involves thousands of files
- If you need to use a script just to use _AP.exe_
- If you have more RAM or CPU than you know what to do with

More than likely if you're stuck we can help 😊.

## What is a _binary_? What is an _executable_? What does _compiling_ mean?

Unlike R, Python, Ruby, or JavaScript, some programming languages can not just be run straight from source code.

We call such languages _compiled_ programming languages because a special program, called a _compiler_
is required to transform the text-based programming source code to a low-level set of instructions that a computer understands.

Some programming languages that need to be compiled include C++, C, Java, and C#.

This compilation step is discrete and happens before the code is run. Compiling is often referred to as _building_.

The result of compilation is one or more _binary_ files. We call these files binaries because the code in them
is no longer readable text--rather it is just blobs of binary instructions that the computer will use.

Binaries that can be run as programs are often called _executables_.

## What is ~~an _action_~~ a _command_?

Our program is a monolith--a very large structure. To support doing different
things we have various sub-programs that can be run by themselves. We call each
of these sub-programs a _command_. If you run _AP.exe_ with no arguments it will
list the available command that you can run.