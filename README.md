# Jeomseon Unity Pooling

JeomseonScriptPack의 풀링 및 풀 스코프 기능을 독립된 Unity Package Manager 패키지로 제공합니다.

## 요구 사항

- Unity 2022.3 이상

## OpenUPM으로 설치

프로젝트의 `Packages/manifest.json`에 OpenUPM scoped registry를 한 번 등록합니다.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon.unity"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.pooling": "0.1.0"
  }
}
```

## Git URL로 설치

Unity Package Manager의 `Install package from git URL`에 다음 URL을 입력합니다.

```text
https://github.com/jeomseon0516/Unity.Pooling.git#v0.1.1
```

## 로컬 개발

개발용 Unity 프로젝트의 `Packages/manifest.json`에 로컬 저장소를 연결합니다.

```json
{
  "dependencies": {
    "com.jeomseon.unity.pooling": "file:../../Jeomseon.Unity.Pooling"
  },
  "testables": [
    "com.jeomseon.unity.pooling"
  ]
}
```

## 포함 기능

- `StringBuilderPool`
- Unity `GameObject` 및 `Component` 오브젝트 풀
- 키 기반 오브젝트 풀
- 풀 반환 시 필드와 프로퍼티를 초기화하는 `PoolInitAttribute`
- 배열, 리스트, 딕셔너리 및 `StringBuilder`용 `IDisposable` 스코프

## 테스트

패키지를 `testables`에 등록한 후 Unity Test Runner의 EditMode에서 실행합니다.

## 라이선스

[MIT License](./LICENSE.md)
