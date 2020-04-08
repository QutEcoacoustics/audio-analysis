// <copyright file="Matrix3D.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;

    public class Matrix3D
    {
        private readonly string[] dimNames = new string[3];
        private readonly int Xdim = 0;
        private readonly int Ydim = 0;
        private readonly int Zdim = 0;
        private readonly float[,,] array3D;

        public Matrix3D(int _Xdim, int _Ydim, int _Zdim)
        {
            this.Xdim = _Xdim;
            this.Ydim = _Ydim;
            this.Zdim = _Zdim;

            this.array3D = new float[_Xdim, _Ydim, _Zdim];
        }

        public Matrix3D(string Xname, int _Xdim, string Yname, int _Ydim, string Zname, int _Zdim)
        {
            this.dimNames[0] = Xname;
            this.dimNames[1] = Yname;
            this.dimNames[2] = Zname;

            this.Xdim = _Xdim;
            this.Ydim = _Ydim;
            this.Zdim = _Zdim;

            this.array3D = new float[_Xdim, _Ydim, _Zdim];
        }

        public void SetValue(int X, int Y, int Z, float value)
        {
            this.array3D[X, Y, Z] = value;
        }

        public void SetValue(int X, int Y, int Z, double value)
        {
            this.array3D[X, Y, Z] = (float)value;
        }

        public void SetValue(int X, int Y, int Z, int value)
        {
            this.array3D[X, Y, Z] = value;
        }

        public float GetValue(int X, int Y, int Z)
        {
            return this.array3D[X, Y, Z];
        }

        // get and set arrays in the X dimension
        public float[] GetArray(char arrayID, int Y, int Z)
        {
            if (arrayID != 'X')
            {
                return null;
            }

            float[] array = new float[this.Xdim];
            for (int X = 0; X < this.Xdim; X++)
            {
                array[X] = this.array3D[X, Y, Z];
            }

            return array;
        }

        public void SetArray(float[] array, int Y, int Z)
        {
            if (array.Length != this.Xdim)
            {
                return;
            }

            for (int X = 0; X < this.Xdim; X++)
            {
                this.array3D[X, Y, Z] = array[X];
            }
        }

        // get and set arrays in the Y dimension
        public float[] GetArray(int X, char arrayID, int Z)
        {
            if (arrayID != 'Y')
            {
                return null;
            }

            float[] array = new float[this.Ydim];
            for (int Y = 0; Y < this.Ydim; Y++)
            {
                array[Y] = this.array3D[X, Y, Z];
            }

            return array;
        }

        public void SetArray(int X, float[] array, int Z)
        {
            if (array.Length != this.Ydim)
            {
                return;
            }

            for (int Y = 0; Y < this.Ydim; Y++)
            {
                this.array3D[X, Y, Z] = array[Y];
            }
        }

        // get and set arrays in the Z dimension
        public float[] GetArray(int X, int Y, char arrayID)
        {
            if (arrayID != 'Z')
            {
                return null;
            }

            float[] array = new float[this.Zdim];
            for (int Z = 0; Z < this.Zdim; Z++)
            {
                array[Z] = this.array3D[X, Y, Z];
            }

            return array;
        }

        public void SetArray(int X, int Y, float[] array)
        {
            if (array.Length != this.Zdim)
            {
                return;
            }

            for (int Z = 0; Z < this.Zdim; Z++)
            {
                this.array3D[X, Y, Z] = array[Z];
            }
        }

        // get and set matrix in the 3D array
        public float[,] GetMatrix(string dimName, int index)
        {
            float[,] matrix;
            if (dimName == this.dimNames[0]) // X dimension
            {
                matrix = new float[this.Ydim, this.Zdim];
                for (int Y = 0; Y < this.Ydim; Y++)
                {
                    for (int Z = 0; Z < this.Zdim; Z++)
                    {
                        matrix[Y, Z] = this.array3D[index, Y, Z];
                    }
                }

                return matrix;
            }
            else
                if (dimName == this.dimNames[1]) // Y dimension
            {
                matrix = new float[this.Xdim, this.Zdim];
                for (int X = 0; X < this.Xdim; X++)
                {
                    for (int Z = 0; Z < this.Zdim; Z++)
                    {
                        matrix[X, Z] = this.array3D[X, index, Z];
                    }
                }

                return matrix;
            }
            else
                    if (dimName == this.dimNames[2]) // Z dimension
            {
                matrix = new float[this.Xdim, this.Ydim];
                for (int X = 0; X < this.Xdim; X++)
                {
                    for (int Y = 0; Y < this.Ydim; Y++)
                    {
                        matrix[X, Y] = this.array3D[X, Y, index];
                    }
                }

                return matrix;
            }

            return null;
        }

        public void SetMatrix(string dimName, int index, float[,] matrix)
        {
            if (dimName == this.dimNames[0]) // X dimension
            {
                for (int Y = 0; Y < this.Ydim; Y++)
                {
                    for (int Z = 0; Z < this.Zdim; Z++)
                    {
                        this.array3D[index, Y, Z] = matrix[Y, Z];
                    }
                }
            }
            else
                if (dimName == this.dimNames[1]) // Y dimension
            {
                for (int X = 0; X < this.Xdim; X++)
                {
                    for (int Z = 0; Z < this.Zdim; Z++)
                    {
                        this.array3D[X, index, Z] = matrix[X, Z];
                    }
                }
            }
            else
                    if (dimName == this.dimNames[2]) // Z dimension
            {
                for (int X = 0; X < this.Xdim; X++)
                {
                    for (int Y = 0; Y < this.Ydim; Y++)
                    {
                        this.array3D[X, Y, index] = matrix[X, Y];
                    }
                }
            }
        }

        public static void Write3DMatrix(Matrix3D M)
        {
            for (int X = 0; X < M.Xdim; X++)
            {
                for (int Y = 0; Y < M.Ydim; Y++)
                {
                    for (int Z = 0; Z < M.Zdim; Z++)
                    {
                        Console.Write("{0:f1} ", M.array3D[X, Y, Z]);
                    }

                    Console.Write("    ");
                }

                Console.WriteLine();
            }
        }

        //TestMatrix3D class

        public static void TestMatrix3dClass()
        {
            string Xname = "XX";
            int Xdim = 4;
            string Yname = "YY";
            int Ydim = 3;
            string Zname = "ZZ";
            int Zdim = 3;
            Matrix3D M = new Matrix3D(Xname, Xdim, Yname, Ydim, Zname, Zdim);

            // set values
            for (int X = 0; X < Xdim; X++)
            {
                for (int Y = 0; Y < Ydim; Y++)
                {
                    for (int Z = 0; Z < Zdim; Z++)
                    {
                        M.array3D[X, Y, Z] = 0.1f;
                    }
                }
            }

            // set matrix
            float[,] matrix = new float[Xdim, Ydim];
            for (int X = 0; X < Xdim; X++)
            {
                for (int Y = 0; Y < Ydim; Y++)
                {
                    matrix[X, Y] = 4.0f;
                }
            }

            M.SetMatrix("ZZ", 1, matrix);

            // set array
            float[] array = { 0.5f, 0.5f, 0.5f, 0.5f };
            M.SetArray(array, 0, 2);

            Write3DMatrix(M);
        }
    }
}