using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
   

    private bool hasGameFinished;

    [SerializeField] private float _rotateSpeed;
    [SerializeField] private float _minX, _maxX, _minY, maxY;


    private void Start()
    {
        hasGameFinished = false;

        transform.position = new Vector3(
            Random.Range(_minX,_maxX),
            Random.Range(_minY,maxY),
            0f);

    }

    private void OnEnable()
    {
        GameManager.Instance.GameEnded += OnGameEnded;
    }

    private void OnDisable()
    {
        GameManager.Instance.GameEnded -= OnGameEnded;
    }


    private void FixedUpdate()
    {
        if (hasGameFinished) return;

        transform.Rotate(_rotateSpeed * Time.fixedDeltaTime * Vector3.forward);
    }

    public void OnGameEnded()
    {
        GetComponent<Collider2D>().enabled = false;
        hasGameFinished = true;
        StartCoroutine(Rescale());
    }

    [SerializeField]
    private float _destroyTime;

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
