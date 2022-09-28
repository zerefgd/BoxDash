using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{

    private bool canMove;
    private bool canShoot;

    [SerializeField]
    private AudioClip _moveClip, _pointClip, _scoreClip, _loseClip;

    [SerializeField]
    private GameObject _explosionPrefab;

    private void Awake()
    {
        canShoot = false;
        canMove = false;
        canRotate = false;

        currentRotateAngle = 90f;
    }

    private void OnEnable()
    {
        GameManager.Instance.GameStarted += GameStarted;
        GameManager.Instance.GameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameManager.Instance.GameStarted -= GameStarted;
        GameManager.Instance.GameEnded -= OnGameEnded;
    }

    private void GameStarted()
    {
        canMove = true;
        canShoot = true;

        minRotateAngle = _startRotateMinAngle;
        maxRotateAngle = _startRotateMaxAngle;
    }


    private void Update()
    {
        if (canShoot && Input.GetMouseButtonDown(0))
        {
            Shoot();
            AudioManager.Instance.PlaySound(_moveClip);
        }
    }

    private void Shoot()
    {

        canRotate = false;
        canShoot = false;

        moveDirection = new Vector3(Mathf.Cos(currentRotateAngle * Mathf.Deg2Rad),
            Mathf.Sin(currentRotateAngle * Mathf.Deg2Rad), 0f);
        currentRotateAngle = (currentRotateAngle + 180f) % 720f;
        _rotateTransform.gameObject.SetActive(false);
    }


    [SerializeField] private Transform _rotateTransform;
    [SerializeField] private float _rotateSpeed, _moveSpeed;
    [SerializeField] private float _startRotateMinAngle, _startRotateMaxAngle;

    private bool canRotate;
    private float currentRotateAngle;
    private Vector3 moveDirection;

    private float minRotateAngle, maxRotateAngle;

    private void FixedUpdate()
    {

        if (!canMove) return;

        if (canRotate)
        {
            if(currentRotateAngle > maxRotateAngle)
            {
                currentRotateAngle = maxRotateAngle;
                _rotateSpeed *= -1f;
            }

            if (currentRotateAngle < minRotateAngle)
            {
                currentRotateAngle = minRotateAngle;
                _rotateSpeed *= -1f;
            }

            currentRotateAngle += _rotateSpeed * Time.fixedDeltaTime;
            _rotateTransform.rotation = Quaternion.Euler(0, 0, currentRotateAngle);
        }
        else if (canMove)
        {
            transform.position += _moveSpeed * Time.fixedDeltaTime * moveDirection;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if(collision.CompareTag(Constants.Tags.SCORE))
        {
            collision.gameObject.GetComponent<Score>().OnGameEnded();
            GameManager.Instance.UpdateScore();
            AudioManager.Instance.PlaySound(_scoreClip);
        }

        if(collision.CompareTag(Constants.Tags.OBSTACLE))
        {
            Destroy(Instantiate(_explosionPrefab,transform.position,Quaternion.identity), 3f);
            AudioManager.Instance.PlaySound(_loseClip);
            GameManager.Instance.EndGame();
        }
        if(collision.CompareTag(Constants.Tags.MOVE))
        {
            AudioManager.Instance.PlaySound(_pointClip);
            canRotate = true;
            canShoot = true;
            _rotateTransform.gameObject.SetActive(true);
            _rotateTransform.rotation = Quaternion.Euler(0, 0, currentRotateAngle);
            var moveScript = collision.gameObject.GetComponent<Boundary>();
            minRotateAngle = moveScript.MinAngle;
            maxRotateAngle = moveScript.MaxAngle;
        }
    }

    [SerializeField] private float _destroyTime;

    public void OnGameEnded()
    {
        StartCoroutine(Rescale());
    }

    private IEnumerator Rescale()
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        Vector3 scaleOffset = endScale - startScale;
        float timeElapsed = 0f;
        float speed = 1 / _destroyTime;
        var updateTime = new WaitForFixedUpdate();
        while (timeElapsed < 1f)
        {
            timeElapsed += speed * Time.fixedDeltaTime;
            transform.localScale = startScale + timeElapsed * scaleOffset;
            yield return updateTime;
        }

        Destroy(gameObject);
    }
}