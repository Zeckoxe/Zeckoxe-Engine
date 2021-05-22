﻿// Copyright (c) 2019-2021 Faber Leonardo. All Rights Reserved. https://github.com/FaberSanZ
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)



using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Vulkan;
using Vultaik.Desktop;
using Vultaik.GLTF;
using Vultaik;
using Vultaik.Physics;
using Buffer = Vultaik.Buffer;
using Interop = Vultaik.Interop;
using Samples.Common;

namespace Samples.LoadGLTF
{


    public class LoadGLTFExample : IDisposable
    {

        public Camera Camera { get; set; }
        public PresentationParameters Parameters { get; set; }
        public Adapter Adapter { get; set; }
        public Device Device { get; set; }
        public Framebuffer Framebuffer { get; set; }
        public SwapChain SwapChain { get; set; }
        public GraphicsContext Context { get; set; }
        public Matrix4x4 Model { get; set; }
        public Window? Window { get; set; }
        public Camera camera { get; set; }


        public ModelAssetImporter<VertexPositionNormal> GLTFModel { get; set; }


        public Buffer ConstBuffer;
        public DescriptorSet DescriptorSet { get; set; }
        public GraphicsPipeline PipelineState;


        public LoadGLTFExample() : base()
        {

        }



        // TransformUniform 
        public TransformUniform uniform;
        public float yaw;
        public float pitch;
        public float roll;



        public void Initialize()
        {
            Window = new Window("Vultaik", 1200, 800);

            Parameters = new PresentationParameters()
            {
                BackBufferWidth = Window.Width,
                BackBufferHeight = Window.Height,
                Settings = new Settings()
                {
                    Validation = ValidationType.None,
                    Fullscreen = false,
                    VSync = false,
                },
            };



            Adapter = new Adapter(Parameters);

            Device = new Device(Adapter);

            SwapChain = new SwapChain(Device, new()
            {
                Source = GetSwapchainSource(),
                ColorSrgb = false,
                Height = Window.Height,
                Width = Window.Width,
                SyncToVerticalBlank = false,
                DepthFormat = Adapter.DepthFormat is VkFormat.Undefined ? null : Adapter.DepthFormat
            });


            Context = new GraphicsContext(Device);
            Framebuffer = new Framebuffer(SwapChain);

            camera = new Camera(45f, 1f, 0.1f, 64f);
            camera.SetPosition(0, 0, -4.0f);
            camera.AspectRatio = (float)Window.Width / Window.Height;
            camera.Update();


            // Reset Model
            Model = Matrix4x4.Identity;

            uniform = new(camera.Projection, Model, camera.View);


            ConstBuffer = new(Device, new()
            {
                BufferFlags = BufferFlags.ConstantBuffer,
                Usage = GraphicsResourceUsage.Dynamic,
                SizeInBytes = Interop.SizeOf<TransformUniform>(),
            });


            CreatePipelineState();

            GLTFModel = new(Device, Constants.ModelsFile + "DamagedHelmet.gltf");

            yaw = 0f;
            pitch = 0;
            roll = 0;
        }

        public void CreatePipelineState()
        {


            string shaders = Constants.ShadersFile;


            PipelineStateDescription pipelineStateDescription = new();
            pipelineStateDescription.SetFramebuffer(Framebuffer);
            pipelineStateDescription.SetShader(new ShaderBytecode(shaders + "LoadGLTF/Fragment.hlsl", ShaderStage.Fragment));
            pipelineStateDescription.SetShader(new ShaderBytecode(shaders + "LoadGLTF/Vertex.hlsl", ShaderStage.Vertex)); 
            pipelineStateDescription.SetVertexBinding(VkVertexInputRate.Vertex, VertexPositionNormal.Size);
            pipelineStateDescription.SetVertexAttribute(VertexType.Position);
            pipelineStateDescription.SetVertexAttribute(VertexType.Color);
            PipelineState = new(pipelineStateDescription);

            DescriptorData descriptorData = new();
            descriptorData.SetUniformBuffer(0, ConstBuffer);
            DescriptorSet = new DescriptorSet(PipelineState, descriptorData);
        }



        public void Update()
        {

            Model = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix4x4.CreateTranslation(0.0f, .0f, 0.0f);
            uniform.Update(camera, Model);
            ConstBuffer.SetData(ref uniform);

            yaw += 0.0006f * MathF.PI;
        }




        public void Draw()
        {

            Device.WaitIdle();
            CommandBuffer commandBuffer = Context.CommandBuffer;
            commandBuffer.Begin();


            commandBuffer.BeginFramebuffer(Framebuffer);
            commandBuffer.SetScissor(Window.Width, Window.Height, 0, 0);
            commandBuffer.SetViewport(Window.Width, Window.Height, 0, 0);

            commandBuffer.SetGraphicPipeline(PipelineState);
            commandBuffer.BindDescriptorSets(DescriptorSet);

            commandBuffer.SetVertexBuffers(new[] { GLTFModel.VertexBuffer });
            commandBuffer.SetIndexBuffer(GLTFModel.IndexBuffer, 0, GLTFModel.IndexType);


            foreach (Scene sc in GLTFModel.Scenes)
            {
                foreach (Node node in sc.Root.Children)
                {
                    RenderNode(commandBuffer, node, sc.Root.LocalMatrix);
                }
            }


            commandBuffer.Close();
            commandBuffer.Submit();
            SwapChain.Present();

        }

        public void RenderNode(CommandBuffer cmd, Node node, Matrix4x4 currentTransform)
        {
            Matrix4x4 localMat = node.LocalMatrix * currentTransform;

            cmd.PushConstant<Matrix4x4>(PipelineState, ShaderStage.Vertex, localMat);

            if (node.Mesh is not null)
                foreach (Primitive p in node.Mesh.Primitives)
                    cmd.DrawIndexed(p.IndexCount, 1, p.FirstIndex, p.FirstVertex, 0);


            if (node.Children is null)
                return;

            foreach (Node child in node.Children)
                RenderNode(cmd, child, localMat);
        }


        public void Run()
        {

            Initialize();

            Window?.Show();
            Window.RenderLoop(() =>
            {
                Update();
                Draw();
            });
        }

        public SwapchainSource GetSwapchainSource()
        {
            if (Adapter.SupportsSurface)
            {
                if (Adapter.SupportsWin32Surface)
                    return Window.SwapchainWin32;

                if (Adapter.SupportsX11Surface)
                    return Window.SwapchainX11;

                if (Adapter.SupportsWaylandSurface)
                    return Window.SwapchainWayland;

                if (Adapter.SupportsMacOSSurface)
                    return Window.SwapchainNS;
            }

            throw new PlatformNotSupportedException("Cannot create a SwapchainSource.");
        }




        public void Dispose()
        {
            Adapter.Dispose();
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct TransformUniform
    {
        public TransformUniform(Matrix4x4 p, Matrix4x4 m, Matrix4x4 v)
        {
            P = p;
            M = m;
            V = v;
        }

        public Matrix4x4 M;

        public Matrix4x4 V;

        public Matrix4x4 P;



        public void Update(Camera camera, Matrix4x4 m)
        {
            P = camera.Projection;
            M = m;
            V = camera.View;
        }
    }
}
