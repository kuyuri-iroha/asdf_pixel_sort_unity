using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResizeMethod
{
    Bilinear32x32 = 0,
    Bilinear16x16,
    Bilinear8x8,
    NearestNeighbor32x32,
    NearestNeighbor16x16,
    NearestNeighbor8x8,
    Bicubic32x32,
    Bicubic16x16,
    Bicubic8x8
}

public class PixelSort : MonoBehaviour
{
    [SerializeField]
    ComputeShader sorterKernel = null;

    [SerializeField]
    Texture2D src = null;

    [SerializeField, Range(0.0f, 1.0f)]
    float brightnessThreshold = 0.0f;

    [SerializeField]
    ComputeShader resizeKernel = null;
    [SerializeField]
    float widthScale = 1.0f;
    [SerializeField]
    float heightScale = 1.0f;
    [SerializeField]
    ResizeMethod resizeMethod = ResizeMethod.Bilinear32x32;
    [SerializeField, Range(0.0f, 1.0f)]
    float sharpness = 0.5f;

    int kernelIndexHorizontal;
    int kernelIndexVertical;

    RenderTexture[] buffers;
    Material mat;

    int resizeKernelIndex;
    int groupNumX;
    int groupNumY;
    RenderTexture resizedSrc;

    string GetKernelString(ResizeMethod method)
    {
        string result = null;

        switch(method)
        {
            case ResizeMethod.Bilinear32x32:
                result = "Bilinear32x32";
                break;

            case ResizeMethod.Bilinear16x16:
                result = "Bilinear16x16";
                break;

            case ResizeMethod.Bilinear8x8:
                result = "Bilinear8x8";
                break;

            case ResizeMethod.NearestNeighbor32x32:
                result = "NearestNeighbor32x32";
                break;

            case ResizeMethod.NearestNeighbor16x16:
                result = "NearestNeighbor16x16";
                break;

            case ResizeMethod.NearestNeighbor8x8:
                result = "NearestNeighbor8x8";
                break;

            case ResizeMethod.Bicubic32x32:
                result = "Bicubic32x32";
                break;

            case ResizeMethod.Bicubic16x16:
                result = "Bicubic16x16";
                break;

            case ResizeMethod.Bicubic8x8:
                result = "Bicubic8x8";
                break;
        }

        return result;
    }

    // Start is called before the first frame update
    void Start()
    {
        int width = Mathf.FloorToInt(src.width * widthScale);
        int height = Mathf.FloorToInt(src.height * heightScale);

        kernelIndexHorizontal = sorterKernel.FindKernel("PixelSortKernelHorizontal");
        kernelIndexVertical = sorterKernel.FindKernel("PixelSortKernelVertical");

        buffers = new RenderTexture[2];
        for(var i = 0; i < buffers.Length; i++)
        {
            buffers[i] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32) {enableRandomWrite = true};
            buffers[i].Create();
        }

        mat = GetComponent<Renderer>().sharedMaterial;

        resizeKernelIndex = resizeKernel.FindKernel(GetKernelString(resizeMethod));
        resizeKernel.GetKernelThreadGroupSizes(resizeKernelIndex, out uint threadX, out uint threadY, out uint threadZ);
        groupNumX = Mathf.CeilToInt((float)width / threadX);
        groupNumY = Mathf.CeilToInt((float)height / threadY);

        resizedSrc = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32) {enableRandomWrite = true};
        resizedSrc.Create();

        resizeKernel.SetTexture(resizeKernelIndex, "src", src);
        resizeKernel.SetTexture(resizeKernelIndex, "dest", resizedSrc);
    }

    // Update is called once per frame
    void Update()
    {
        resizeKernel.SetFloat("sharpness", sharpness);
        resizeKernel.Dispatch(resizeKernelIndex, groupNumX, groupNumY, 1);

        Graphics.CopyTexture(resizedSrc, buffers[0]);

        sorterKernel.SetFloat("brightnessThreshold", brightnessThreshold);

        int count = 0;
        for(int i = 0; i < resizedSrc.width; i++)
        {
            sorterKernel.SetFloat("iteration", i);
            sorterKernel.SetTexture(kernelIndexHorizontal, "src", buffers[count % 2]);
            sorterKernel.SetTexture(kernelIndexHorizontal, "dest", buffers[(count + 1) % 2]);
            sorterKernel.Dispatch(kernelIndexHorizontal, 1, resizedSrc.height, 1);

            count++;
        }

        for(int i = 0; i < resizedSrc.height; i++)
        {
            sorterKernel.SetFloat("iteration", i);
            sorterKernel.SetTexture(kernelIndexVertical, "src", buffers[count % 2]);
            sorterKernel.SetTexture(kernelIndexVertical, "dest", buffers[(count + 1) % 2]);
            sorterKernel.Dispatch(kernelIndexVertical, resizedSrc.width, 1, 1);

            count++;
        }

        mat.SetTexture("_UnlitColorMap", buffers[count % 2]);
    }
}
