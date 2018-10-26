# dll 파일 Embedding

현재는 Fody를 사용하므로 이 방법은 사용하지 않는다.

``` c#
static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
{
    Assembly thisAssembly = Assembly.GetExecutingAssembly();
    var name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";

    string[] names = thisAssembly.GetManifestResourceNames();
    var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(name));
    if (resources.Count() > 0)
    {
        string resourceName = resources.First();
        using (Stream stream = thisAssembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                byte[] assembly = new byte[stream.Length];
                stream.Read(assembly, 0, assembly.Length);
                return Assembly.Load(assembly);
            }
        }
    }
    return null;
}
```

메인스레드 호출전(`Program.cs:Main` 함수)에 이 코드를 추가한다.

``` c#
AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ResolveAssembly);
```

이제 리소스에 dll 파일을 넣고 `FileType`을 `Binary`로 바꾸면 된다. 이제 dll 파일이 실행파일에 포함될 것이다.