﻿using System;

namespace VL.Audio
{
    public class GistSignal : SinkSignal
    {
        Gist FGist;
        
        void InitGist()
        {
            if(FGist != null)
            {
                FGist.Dispose();
            }
            
            if(BufferSize > 2 && SampleRate > 0)
                FGist = new Gist(SampleRate, BufferSize);
        }
        
        //SigParamDiff<int> GistBufferSize = new SigParamDiff<int>("Gist Buffer Size");
        SigParam<bool> GetFeatures = new SigParam<bool>("GetFeatures", true, false);

        //SigParam<float> RMS = new SigParam<float>("RMS", true);
        SigParam<float[]> Features = new SigParam<float[]>("Features", true);
        SigParam<float[]> FFT = new SigParam<float[]>("Spectrum", true);
        SigParam<string> FeatureNames = new SigParam<string>("Feature Names", true);

        public GistSignal()
        {
            //GistBufferSize.ValueChanged = v => v
            FeatureNames.Value = string.Join(Environment.NewLine, Enum.GetNames(typeof(AudioFeaturesFlags)));
        }
        
        protected override void Engine_SampleRateChanged(object sender, EventArgs e)
        {
            base.Engine_SampleRateChanged(sender, e);
            
            InitGist();
        }
        
        protected override void Engine_BufferSizeChanged(object sender, EventArgs e)
        {
            base.Engine_BufferSizeChanged(sender, e);
            
            InitGist();
        }
        
        protected override void FillBuffer(float[] buffer, int offset, int count)
        {
            if (FGist != null)
            {
                InputSignal.Read(buffer, offset, count);
                //base.FillBuffer(buffer, offset, count);
                FGist.ProcessFrame(buffer, count);
                FFT.Value = FGist.SpectrumData;

                //get flags
                AudioFeaturesFlags flags = (AudioFeaturesFlags)2047;

                var features = FGist.GetFeatures(flags);
                var vals = new float[11];
                features.Values.CopyTo(vals, 0);
                Features.Value = vals; 

            }
            
        }
        
        public override void Dispose()
        {
            if(FGist != null)
            {
                FGist.Dispose();
            }
            
            base.Dispose();
        }
    }
}