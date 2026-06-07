using DamageNumbersPro;
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
        [SerializeField]
        private DamageNumberMesh _damageNumber;

        private const string BonusText = "+1";
        
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
            _damageNumber.Spawn(GetRandomPosition(), BonusText);
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

        private Vector3 GetRandomPosition()
        {
            Vector3 xOffset = Vector3.right * Random.Range(0, 6) * .2f * (Random.Range(0, 2) * 2 - 1);
            Vector3 yOffset = Vector3.up * (Random.Range(0, 6) * .05f + .75f);
            return transform.position + xOffset + yOffset;
        }
    }
}