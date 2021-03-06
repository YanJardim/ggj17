﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Antenna : MonoBehaviour {
    public bool start;
    public bool active;

    private float lifeTime ;
    public float angle = 1;
    public int power = 10;
    public int gap = 1;
    public int itensVisible = 0;
    public float rayCount = 1;
    public bool debug = false;
    public Vector3 dir;

    public const float multiplier = 5;

    public LayerMask mask;

    private List<Antenna> oldVisibleObjs = new List<Antenna>();
    private List<Antenna> visibleObjs= new List<Antenna>();

    private BoxCollider box;

    public bool canDelete, delete;

    Quaternion startingAngle = Quaternion.AngleAxis(0, Vector3.up);
    Quaternion stepAngle = Quaternion.AngleAxis(-5, Vector3.up);
    private ParticleSystem tpc;
void DetectThings()
    {
        RaycastHit hit; //variavel que recebe um objeto
        var angle = transform.rotation * Quaternion.AngleAxis(0, Vector3.up);
        var direction = angle * Vector3.forward;
        var pos = transform.TransformPoint(box.center);
        //CERTO

        
        int  entradas= 0;
        foreach (Antenna g in visibleObjs)
        {
            
            if (!g.start)
            {
                g.active = false;
                g.itensVisible = 0;
                ParticleSystem otherSystem = g.transform.GetChild(0).GetComponent<ParticleSystem>();
                otherSystem.enableEmission = false;
            }
           
        }

        

            visibleObjs.Clear();
        for (var i = 0; i < rayCount; i++) //faz os ray casts
        {
            if (debug)
            {
                Debug.DrawRay(pos, direction * multiplier * power, Color.red); //desenha raios
            }
            
            if (Physics.Raycast(pos, direction, out hit, power * multiplier)) //faz raycast
            {
                var otherAntenna = hit.collider.GetComponent<Antenna>();    //verifica se uma antena foi atingida

                if (otherAntenna)
                {
                     bool repeated = false;
                    foreach(Antenna g in visibleObjs) //verifica todos os objetos vistos nesse loop
                    {
                        if(g == otherAntenna) //se 2 ou mais raios batem na mesma antena
                        {
                            repeated = true;    //repetiu
                        }
                    }
                
                    if (!repeated) //se não repetiu
                    {
                        entradas ++;
                        otherAntenna.active = true; //ativa a antena e insere na lista de antenas atingidas
                        ParticleSystem otherSystem = otherAntenna.transform.GetChild(0).GetComponent<ParticleSystem>();
                        otherSystem.enableEmission = true;
                        visibleObjs.Add(otherAntenna);
                    }

                }
              /*  var wall = hit.collider.GetComponent<Wall>();
                else if (wall)
                {
                    //MOSTRA QUE A ONDA NÃO IRÁ PASSAR A PAREDE
                }
                */

                
            }
            direction = stepAngle * direction; // prepara o próximo raycast
        }

 
    
        
            bool found = false;
            foreach (Antenna g in visibleObjs)
            {
                foreach (Antenna g1 in oldVisibleObjs)
                {
                    if(g == g1)
                    {
                        found = true;
                      
                    }
                }

                if(found == false)
                {
                    if (!g.start)
                    {
                        g.active = false; //desativa a antena
                        ParticleSystem otherSystem = g.transform.GetChild(0).GetComponent<ParticleSystem>();
                        otherSystem.enableEmission = false;
                        g.itensVisible = 0;
                }
                    
                    g.oldVisibleObjs.Clear();
                    g.visibleObjs.Clear();
                    oldVisibleObjs.Remove(g);
                }
                    //se antena antiga não foi encontrada na lista atual

                    
                   
                   
                

                found = false; //supõe que o próximo elemento da lista anterior está na atual.

            }
       

        oldVisibleObjs = visibleObjs; //sobreescreve a lista antiga
     //   visibleObjs.Clear();
    }


    // Use this for initialization
    void Start () {
        lifeTime = 3.6f;
        dir = transform.forward * multiplier * power;
        box = GetComponent<BoxCollider>();
        
        Ray r = new Ray(transform.TransformPoint(box.center), dir);
  
    tpc = transform.GetChild(0).GetComponent<ParticleSystem>();
        if(!active)
        {
            tpc.enableEmission = false;

        }
    mask = 1 << LayerMask.NameToLayer("Antenna") | LayerMask.NameToLayer("Glass");

        canDelete = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (active && gameObject != null) //se torre está ativa
        {
            dir = transform.forward * multiplier * power;
            RayManager();
            canDelete = false;
            DetectThings();
            canDelete = true;
            itensVisible = visibleObjs.Count;   // adquire o número de objetos vistos
            transform.rotation = Quaternion.Euler(0, angle, 0);

            power = gap <= 44 ? 10 : 10 - (gap / 45);
  


        }

        
        else if (!start)
        {
            foreach (Antenna a in visibleObjs)
            {
                if (!a.start)
                {
                    a.active = false;
                    ParticleSystem otherSystem = a.transform.GetChild(0).GetComponent<ParticleSystem>();
                    otherSystem.enableEmission = false;
                    
                    a.itensVisible = 0;
                }

                a.oldVisibleObjs.Clear();
                a.visibleObjs.Clear();
            }

            foreach (Antenna a in oldVisibleObjs)
            {
                if (!a.start)
                {
                    a.active = false;
                    ParticleSystem otherSystem = a.transform.GetChild(0).GetComponent<ParticleSystem>();
                    otherSystem.enableEmission = false;
 ;
                    a.itensVisible = 0;
                }
                a.oldVisibleObjs.Clear();
                a.visibleObjs.Clear();
            }
           
        }

        gap = Mathf.Clamp(gap, 1, 360);
        power = Mathf.Clamp(power, 3, 10);
        if (!tpc.Equals(null))
        {
            ParticleSystem.ShapeModule sh = tpc.shape;
            sh.arc = gap;
            ParticleSystem.Particle[] parts;
            parts = new ParticleSystem.Particle[tpc.maxParticles]; 
            int numPartsAlive = tpc.GetParticles(parts);
            for (int i = 0; i < numPartsAlive; i++)
            {
                parts[i].startLifetime =(float) 3.6 * power / multiplier;
                
            }
            tpc.SetParticles(parts, numPartsAlive);

        }


        if (delete && canDelete)
        {
            Destroy(this.gameObject);
        }



    }

    public void RayManager()
    {

        rayCount = gap / 5;
        if (gap % 5 != 0 && gap >= 1) {
            rayCount = (int) Mathf.Ceil(rayCount) + 1;
        }


    }

}