using UnityEngine;
using UnityEngine.AI; // مكتبة الذكاء الاصطناعي

public class WaypointPatrol : MonoBehaviour
{
    public NavMeshAgent agent;       // المكون المسؤول عن الحركة
    public Animator animator;        // المكون المسؤول عن الأنيميشن
    public Transform[] waypoints;    // مصفوفة لنقاط المسار
    
    int currentPointIndex = 0;       // عداد: احنا رايحين انهي نقطة؟

    void Start()
    {
        // إلغاء التحديث التلقائي للدوران عشان الأنيميشن ميبوظش (اختياري، جرب تشيله لو الحركة غريبة)
        // agent.updateRotation = true; 
        
        // التحرك لأول نقطة فوراً
        if(waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
            // تشغيل أنيميشن المشي
            animator.SetBool("IsWalking", true);
        }
    }

    void Update()
    {
        // حساب المسافة المتبقية للوصول للنقطة
        // لو المسافة أقل من نص متر، يبقى وصل
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPoint();
        }
    }

    void GoToNextPoint()
    {
        // لو مفيش نقاط، ميعملش حاجة
        if (waypoints.Length == 0) return;

        // نقل العداد للنقطة اللي بعدها
        // علامة % دي عشان لما يوصل لآخر نقطة يرجع لأول نقطة (0) تاني
        currentPointIndex = (currentPointIndex + 1) % waypoints.Length;

        // إعطاء الأمر بالذهاب للنقطة الجديدة
        agent.SetDestination(waypoints[currentPointIndex].position);
    }
}