using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VVVV.Audio
{
    public class BTrackSignal : SinkSignal
    {
        private BTrack FBTrack;

        private bool isValid = false;
        private bool beatReadFlag = false;
        private bool fixedTempo = false;
        
        void InitBTrack()
        {
            isValid = false;

            if (FBTrack != null)
            {
                FBTrack.Dispose();
                FBTrack = null;
            }

            // TODO: warn, when samplerate is not 44100 Hz
            if (SampleRate == 44100 && BufferSize >= 2)
            {
                FBTrack = new BTrack(/*int)(BufferSize * 0.5)*/BufferSize, BufferSize);
                isValid = true;
            }
                
        }


        SigParam<bool> Beat = new SigParam<bool>("Beat", false, true);
        SigParam<double> Bpm = new SigParam<double>("BPM", true);

        public BTrackSignal()
        {
            InitBTrack();
        }

        protected override void Engine_SampleRateChanged(object sender, EventArgs e)
        {
            base.Engine_SampleRateChanged(sender, e);

            InitBTrack();
        }

        protected override void Engine_BufferSizeChanged(object sender, EventArgs e)
        {
            base.Engine_BufferSizeChanged(sender, e);

            InitBTrack();
        }

        public void invalidate()
        {
            isValid = false;
        }

        protected override void FillBuffer(float[] buffer, int offset, int count)
        {
            if (FBTrack != null && isValid)
            {
                InputSignal.Read(buffer, offset, count);
                //base.FillBuffer(buffer, offset, count);
                FBTrack.ProcessFrame(buffer, count);

                if (FBTrack.Beat)
                {
                    beatReadFlag = false;
                }
                Beat.Value = beatReadFlag ? false : true;   // return true as long as beat was not yet registered by node
                Bpm.Value = FBTrack.Bpm;
            }
        }

        public void NotifyBeatRead()
        {
            beatReadFlag = true;
        }

        public void NotifyFixedTempo(bool fixedTempo, double tempo)
        {
            FBTrack.FixTempo(fixedTempo, tempo);
        }

        public void NotifyTempoChanged(double tempo)
        {
            FBTrack.SetTempo(tempo);
        }

        public void NotifyAlpha(double alpha)
        {
            FBTrack.SetAlpha(alpha);
        }

        public void NotifyODFSample(double sample)
        {
            FBTrack.ProcessOnsetDetectionFunctionSample(sample);
        }

        public override void Dispose()
        {
            if (FBTrack != null)
            {
                isValid = false;
                FBTrack.Dispose();
                FBTrack = null;
            }
            
            base.Dispose();
        }
    }

    
}
