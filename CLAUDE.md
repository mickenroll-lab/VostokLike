# VostokLike 開発引継ぎ

## プロジェクト概要
Unity 2022.3.62f3 / Built-in RendererでEscape from TarkovライクなシングルプレイFPSを開発中。

## 開発ルール
1. コード修正は部分修正と該当箇所明示を優先
2. Inspector操作はオブジェクト名から始める
3. 将来的な弊害可能性を提示してから実装提案
4. 確認リストのリマインダーを定期的に出す

## ゲームサイクル
1. セーフゾーンで装備準備
2. 出撃してフィールドへ
3. 探索・戦闘・アイテム収集
4. 脱出ポイントから帰還 → 戦利品確定
5. セーフゾーンに戻る

## 主要スクリプト
- `RaidManager.cs`：レイドのライフサイクル管理（BeginRaid/EndRaid）
- `PlayerState.cs`：HP・hunger・thirst管理、死亡/リスポーン
- `Inventory.cs`：アイテム管理、グリッドUI、justOpenedフラグでFキー2重検知防止
- `BoxContainer.cs`：BoxGridのUI・アイテム操作、isOpenフラグ、CloseBoxメソッド
- `EquipmentSlot.cs`：装備スロット管理
- `EnemyAI.cs`：敵AI（Patrol/Alert/Search/Chase/Attack）
- `Gun.cs`：射撃・リロード・カメラシェイク
- `ExitZone.cs`：出撃・帰還・LootContainerインタラクト
- `LootContainer.cs`：ルートコンテナ（ScriptableObject方式、開閉ロジック実装済み）
- `SpawnManager.cs`：プレイヤースポーン管理
- `EnemySpawnManager.cs`：敵スポーン管理

## 実装済み機能
- BoxGrid/PlayerGridのD&D（装備スロット含む）
- PrimaryAction（第一アクション：Weapon=装備、Consumable=消費、Bullet=何もしない）
- 敵AI状態遷移（Patrol→Alert→Search→Chase→Attack）
- カメラシェイク・マズルフラッシュ
- RaidManager（死亡・帰還の統一処理）
- LootContainer（ScriptableObject方式、Fキーで開閉、2重検知バグ修正済み）

## 確認リスト（未対応）
- [ ] Curious状態の追加（敵AI）
- [ ] `RaidRuntimeRoot` の導入
- [ ] `ItemData`取得ユーティリティのまとめ
- [ ] `DragSource` enumへのリファクタリング
- [ ] BoxGridの任意セルへのD&D
- [ ] アイテムスポーン管理の完成

## ItemData カテゴリ
```csharp
public enum ItemCategory { Weapon, Consumable, Bullet }
```

## 敵AI状態遷移
```
Patrol → 被弾 → Alert（20秒）→ Patrol
Patrol/Alert → CanSeePlayer → Chase → Attack
Chase → 見失う → Search（20秒）→ Alert → Patrol
Attack → 視線切れ → Search
```

## 報告フォーマット（Claude Code用）

修正報告は以下の順番で書くこと：

### 1. 結論（1〜2行）
何が原因で、何を直したかを一言で。

### 2. プレイヤーが見える動作で説明
コードの話をする前に、ゲーム上で何が起きていたかを説明する。
例：「Fキーを押すとBoxGridが開いた瞬間に閉じていた」

### 3. 技術的な補足（任意）
なぜそうなっていたかの技術的な理由を短く。
コードブロックは最小限にする。

### 4. 修正内容
何のファイルの何を変えたかを箇条書きで。

### 5. 確認してほしいこと
プレイヤーが実際にゲームで確認できる内容のみ書く。
「ログで〇〇が出るか確認」ではなく「〇〇の動作を確認してください」と書く。

---

### 悪い例
「`justOpened`フラグが立っていてもその後すぐにreturnせずにリセットしていたため、
同フレーム内のFキー入力を無視し...」

### 良い例
「Fキーを押すと同じ瞬間にBoxGridが開いて閉じていました。
修正後はFキー1回でBoxGridが開くようになります。
フィールドのCubeにFキーでBoxGridが開くか確認してください。」