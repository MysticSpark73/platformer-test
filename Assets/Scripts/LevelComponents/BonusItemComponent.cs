using DG.Tweening;
using PlayerControl;
using UnityEngine;

namespace LevelComponents
{
    public class BonusItemComponent : BasePlayerTriggerComponent
    {
        [SerializeField]
        private float _animationDuration = 1f;
        [SerializeField]
        private float _impactMagnitude = .5f;

        private Sequence _sequence;
        private bool _isAnimating;
        

        protected override void OnPlayerEnterAction(IPlayerObject playerObject)
        {
            if (_isAnimating) return;
            SpawnScore();
            PlayBounceAnimation(playerObject);
        }

        private void SpawnScore()
        {
            
        }

        private void PlayBounceAnimation(IPlayerObject playerObject)
        {
            Transform playerTransform = playerObject.GetTransform();
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            
            if (_sequence != null)
            {
                _sequence.Kill(true);
            }

            _sequence = CreateSequence(-direction);
            _sequence.Play();
        }

        private Sequence CreateSequence(Vector3 direction)
        {
            _sequence = DOTween.Sequence();
            _sequence.Append(transform.DOMove(transform.position + direction * _impactMagnitude,
                _animationDuration * .5f));
            _sequence.Append(transform.DOMove(transform.position, _animationDuration * .5f));
            _sequence.SetEase(Ease.OutCubic);
            _sequence.OnStart(() => _isAnimating = true);
            _sequence.OnComplete(() => _isAnimating = false);
            return _sequence;
        }
    }
}