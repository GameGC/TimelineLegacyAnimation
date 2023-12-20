using System;
using UnityEngine;
using UnityEngine.Playables;

namespace GameGC.Timeline.LegacyAnimation.Runtime
{
    [Serializable]
    public class AnimationLegacyPlayable : PlayableBehaviour
    { 
        public AnimationClip clip;

        public float easeInDuration;
        public float easeOutStartTime;
    
        public bool isBlendIn;
        public bool isBlendOut;
    
        private Animation _component;


    
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Application.isPlaying && !_component)
            {
                _component = info.output.GetUserData() as Animation;
                if (!_component)
                    _component = info.output.GetReferenceObject() as Animation;
            }

            if (ValidateCanPlayRuntime(info))
            {
                if (!_component.enabled)
                    _component.enabled = true;
            
                PlayClip(playable);
            }
        }
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (Application.isPlaying && !_component)
            {
                _component = info.output.GetUserData() as Animation;
                if (!_component)
                    _component = info.output.GetReferenceObject() as Animation;
                
                if(!_component) return;
                
                PlayClip(playable);
            }

#if UNITY_EDITOR
            if (ValidateCanPlayEditor(info))
                clip.SampleAnimation(_component.gameObject, (float) playable.GetTime());
            else
#endif
            if (playable.GetTime() > easeOutStartTime - 0.01f)
                _component.Blend(clip.name, 0, (float) playable.GetDuration() - easeOutStartTime);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if(ValidateCanPlayRuntime(info))
                _component.Stop(clip.name);
        }

        public override void OnGraphStop(Playable playable)
        { 
            if(_component && clip)
                _component.Stop(clip.name);
        }

        private bool ValidateCanPlayRuntime(FrameData info)
        {
            return Application.isPlaying && _component && clip && info.evaluationType == FrameData.EvaluationType.Playback;
        }

#if UNITY_EDITOR
        private bool ValidateCanPlayEditor(FrameData info)
        {
            return !Application.isPlaying && _component && clip &&
                   info.evaluationType == FrameData.EvaluationType.Evaluate;
        }
#endif
    
        private void PlayClip(Playable playable)
        {
            var tempClip = _component.GetClip(clip.name);
            if (tempClip != clip)
            {
                if(tempClip) 
                    _component.RemoveClip(tempClip.name);
                _component.AddClip(clip,clip.name);
            }
            
            float speed = (float)(playable.GetDuration() /(double)clip.length);
            if (Math.Abs(speed - 1f) > 0.01f) 
                _component[clip.name].speed = speed;
            
            if (easeInDuration > 0)
            {
                if(isBlendIn)
                    _component.Blend(clip.name,1,easeInDuration);
                else 
                    _component.CrossFade(clip.name,easeInDuration);
            }
            else
            {
                _component.Play(clip.name);
            }
            
            _component[clip.name].time = (float) playable.GetTime();
        }
    }
}