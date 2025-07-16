using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class BackgroundTiles : MonoBehaviour
{
    public int hitPoints;
    private SpriteRenderer sprite;
    private GoalManager goalManager;

    private void Start()
    {
        goalManager=FindObjectOfType<GoalManager>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (hitPoints <= 0)
        {
            if(goalManager!=null)
            {
                goalManager.CompareGoal(this.gameObject.tag);
                goalManager.UpdateGoals();
            }
            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        hitPoints -= damage;
        makeLighter();
    }
    void makeLighter()
    {
        //take current color
        Color color=sprite.color;
        //get current color's alpha value
        float newAlpha=color.a * .5f;
        sprite.color=new Color(color.r,color.g,color.b,newAlpha);
    }
}
