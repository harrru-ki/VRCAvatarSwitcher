# VRCAvatarSwitcher

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

VRChatでアバターをランダムに切り替える簡易なコマンドラインツールです。VRChat APIを使用して、アバターの自動切り替えを支援します。

このツールは、VRChatの非公式ツールであり、VRChat公式のサポートはありません。使用は自己責任でお願いします。

## 機能

- **getAuthToken**: VRChatにログインして認証トークンを取得します。2FA（二要素認証）が必要です。
- **getOwnAvatarList**: 自分のアバター一覧を取得し、`own_avatars.txt`に出力します。
- **switchEachAvatar**: アバターを1つずつ試すことができます。使いたくないアバターを`unused_avatars.txt`にリストアップできます。また、特定のワールドで特定のアバターを使いたい場合は、`world_avatar_map.txt`に記載できます。
- **worldAvatarSwitcher**: ワールド移動時に自動でアバターをランダムに切り替えます。`unused_avatars.txt`にリストアップしたアバターは選択されません。`world_avatar_map.txt`に記載されたワールドの場合は、ランダムではなく指定されたアバターが選択されます。
- **revokeAuthToken**: 認証トークンを無効化します。

## 必要条件

- VRChatアカウント（ユーザー名、パスワード、メールアドレス）

## インストール

[最新Releasesのページ](https://github.com/harrru-ki/VRCAvatarSwitcher/releases/latest)からexeファイルをダウンロードしてください。


<details>
<summary>ソースからビルドする場合</summary>

### 必要要件
- .NET 9.0 SDK

### ビルド手順
1. リポジトリをクローンします。
   ```bash
   git clone https://github.com/your-repo/VRCAvatarSwitcher.git
   cd VRCAvatarSwitcher
   ```

2. プロジェクトをビルドします。
   ```bash
    dotnet publish .\getAuthToken\getAuthToken.csproj                 -c Release -r win-x64 /p:PublishAot=true /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\win-x64
    dotnet publish .\getOwnAvatarList\getOwnAvatarList.csproj         -c Release -r win-x64 /p:PublishAot=true /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\win-x64
    dotnet publish .\switchAvatarRandomly\switchAvatarRandomly.csproj -c Release -r win-x64 /p:PublishAot=true /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\win-x64
    dotnet publish .\switchEachAvatar\switchEachAvatar.csproj         -c Release -r win-x64 /p:PublishAot=true /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\win-x64
    dotnet publish .\worldAvatarSwitcher\worldAvatarSwitcher.csproj   -c Release -r win-x64 /p:PublishAot=true /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\win-x64
    dotnet publish .\revokeAuthToken\revokeAuthToken.csproj           -c Release -r win-x64 /p:PublishAot=true /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=true -o publish\win-x64
   ```

3. 各ツールのexeファイルが`publish\win-x64`に生成されます。

</details>

## 使用方法

### ステップバイステップガイド

1. **VRChatのユーザー名とパスワードを控えておきます。**

2. **getAuthTokenで、VRChatにログインして認証トークンを取得します。**
   - ログインするのに、ユーザー名とパスワードとメールアドレスの2FAが必要です。
   - 実行例:
     ```bash
     ./getAuthToken.exe
     ```
   - ユーザー名とパスワードを入力し、2FAコードを入力します。
   - 認証トークンが`auth_token.txt`に保存されます。

3. **getOwnAvatarListで、自分のアバターの一覧を取得します。**
   - 実行例:
     ```bash
     ./getOwnAvatarList.exe
     ```
   - アバター一覧が`own_avatars.txt`に出力されます。フォーマット: `アバターID,アバター名`

4. **switchEachAvatarで、アバターを1つずつ試します。**
   - 使いたくないアバターを`unused_avatars.txt`にリストアップしましょう（1行に1つのアバターID）。
   - 特定のワールドで特定のアバターを使いたい場合は、`world_avatar_map.txt`に記載しましょう（フォーマット: `ワールドID,アバターID`）。
   - 実行例:
     ```bash
     ./switchEachAvatar.exe
     ```
   - エンターキーを押すたびに次のアバターに切り替わります。Q + エンターで終了。

5. **worldAvatarSwitcherで、ワールド移動時に自動でアバターを切り替えます。**
   - `unused_avatars.txt`にリストアップしたアバターは選択されません。
   - `world_avatar_map.txt`に記載されたワールドの場合は指定されたアバターが選択されます。
   - 実行例:
     ```bash
     ./worldAvatarSwitcher.exe
     ```
   - ツールが実行中は、30秒ごとにワールドをチェックし、変更時にアバターを切り替えます。

### 設定ファイルのフォーマット

- **own_avatars.txt**: アバター一覧。CSV形式。
  ```
  avtr_12345678-1234-1234-1234-123456789abc,My Avatar
  avtr_87654321-4321-4321-4321-cba987654321,Another Avatar
  ```

- **unused_avatars.txt**: 使用しないアバターのIDリスト。1行に1つ。
  ```
  avtr_12345678-1234-1234-1234-123456789abc
  ```

- **world_avatar_map.txt**: ワールドごとのアバター指定。CSV形式。
  ```
  wrld_abcdef12-3456-7890-abcd-ef1234567890,avtr_12345678-1234-1234-1234-123456789abc
  ```

## トラブルシューティング

- **ログインに失敗する**: ユーザー名、パスワード、2FAコードを確認してください。VRChatアカウントが有効か確認。
- **APIエラー**: VRChat APIのレートリミットに引っかかっている可能性があります。時間を置いて再試行。
- **トークンが無効**: `revokeAuthToken`でトークンを無効化し、再取得してください。

## 注意事項

- 認証トークンは漏洩しないよう注意してください。
- 定期的に**revokeAuthToken**と**getAuthToken**で認証トークンを無効化・再取得するようにしてください。
  - 本ツールは、認証トークンの有効期限を明に指定していません。
  - つまり、大抵の場合認証トークンが漏洩したことに気づけないので、無期限に認証トークンを悪用され続ける可能性があります。
- このツールは非公式であり、VRChat公式のサポートはありません。

## ライセンス

このプロジェクトはMITライセンスの下で公開されています。詳細は[LICENSE.txt](LICENSE.txt)をご覧ください。
