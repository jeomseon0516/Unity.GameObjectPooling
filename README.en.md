# Jeomseon Unity Pooling

Provides the pooling and pooling-scope utilities from JeomseonScriptPack as an independent Unity Package Manager package.

## Requirements

- Unity 2022.3 or newer

## Installation with OpenUPM

Register the OpenUPM scoped registry once in your project's `Packages/manifest.json`.

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

## Git URL

Use the following URL with Unity Package Manager's `Install package from git URL` option:

```text
https://github.com/jeomseon0516/Unity.Pooling.git#v0.1.0
```

## Features

- `StringBuilderPool`
- Unity `GameObject` and `Component` object pools
- Keyed object pools
- `PoolInitAttribute` for resetting fields and properties on return
- Disposable scopes for arrays, lists, dictionaries, and `StringBuilder`

## Dependencies

This package has no dependency on other Jeomseon packages.

## License

[MIT License](./LICENSE.md)
