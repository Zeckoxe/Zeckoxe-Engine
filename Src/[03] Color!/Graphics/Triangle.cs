﻿using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace _03__Color_
{
    public class Triangle
    {
        // Properties
        private Buffer VertexBuffer { get; set; }

        public int VertexCount { get; set; }



        // Methods.
        public void InitializeBuffer(Device Device)
        {
            VertexCount = 3;
            Vertex[] Vertices = new Vertex[VertexCount];
            Vertices[0] = new Vertex(new Vector3(0.0f, 0.7f, 0.0f), Color.Red);
            Vertices[1] = new Vertex(new Vector3(0.7f, -0.7f, 0.0f), Color.Blue);
            Vertices[2] = new Vertex(new Vector3(-0.7f, -0.7f, 0.0f), Color.Green);

            VertexBuffer = Buffer.Create<Vertex>(Device, BindFlags.VertexBuffer, Vertices);

        }


        public void RenderBuffers(DeviceContext deviceContext)
        {
            VertexBufferBinding vertexBuffer = new VertexBufferBinding();
            vertexBuffer.Buffer = VertexBuffer;
            vertexBuffer.Stride = Utilities.SizeOf<Vertex>();
            vertexBuffer.Offset = 0;

            // Set the vertex buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetVertexBuffers(0, vertexBuffer);


            // Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
    }
}
