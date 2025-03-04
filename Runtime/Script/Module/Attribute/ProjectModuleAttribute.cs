
using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ProjectModuleAttribute : Attribute
{
    /// <summary>
    /// 모듈 경로 (메뉴 항목에 표시될 경로)
    /// </summary>
    public string Path { get; private set; }
    
    /// <summary>
    /// 코어 모듈인지 여부 (코어 모듈은 제거 불가)
    /// </summary>
    public bool IsCore { get; private set; }
    
    /// <summary>
    /// 정렬 순서 (높은 값이 먼저 표시됨)
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// 모듈 속성 생성자
    /// </summary>
    /// <param name="path">메뉴 항목에 표시될 경로</param>
    /// <param name="isCore">코어 모듈 여부</param>
    /// <param name="order">정렬 순서</param>
    public ProjectModuleAttribute(string path, bool isCore = false, int order = 0)
    {
        Path = path;
        IsCore = isCore;
        Order = order;
    }
}