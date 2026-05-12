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
- 敵AI状態遷移（Patrol→Curious→Alert→Search→Chase→Attack）
- カメラシェイク・マズルフラッシュ
- RaidManager（死亡・帰還の統一処理）
- LootContainer（ScriptableObject方式、Fキーで開閉、2重検知バグ修正済み）
- BoxGridからのPrimaryAction（Consumable）修正済み（`ApplyConsumableEffect`でインベントリ経由せず直接効果発動）
- 消耗品スタック修正済み（`GetStackLimit`にBeefCan・Water・MedKitの上限5を追加、`AddItem`の既存チェック漏れを修正）
- BoxGrid/PlayerGrid個別セル管理修正済み（消耗品はスタックなし・常に新規セル作成、弾薬のみスタック。`AddItem`は`stackLimit>1`のみ`existing.amount++`、それ以外は毎回`FindFreeSpace`で配置）
- PlayerGrid→BoxGrid移動の2重削除バグ修正済み（`MoveToBox`の余分なInventoryItem削除ブロックを除去、moveAll時は`FindAll`で全件削除、Shift+クリックをmoveAll=falseに統一）
- Shift+クリック移動が常に先頭アイテムを選ぶバグ修正済み（`MoveToBoxItem`を追加、クリックされたセルの`InventoryItem`を直接キャプチャして渡す）
- LootContainer再度開くと補充されるバグ修正済み（`BoxContainer`に`currentLootContainer`を追加、`MoveToPlayer`時に`RemoveFromContents`で本体dictを同期）
- LootContainerをFキーで閉じた後に再インタラクトできないバグ修正済み（`Inventory.Update()`のFキー処理で`boxContainer.CloseBox()`を呼ぶよう変更し`isOpen`フラグを正しくリセット）
- TabキーでBoxGrid閉じた後LootContainerに再インタラクトできないバグ修正済み（`Inventory.Update()`のTabキー処理にも`boxContainer.CloseBox()`を追加）
- LootContainerのレイド間リセット実装済み（`ResetRaidWorld`で`FindObjectsOfType<LootContainer>()`を全走査して`ResetContainer()`呼び出し）
- `RaidRuntimeRoot` の導入済み（`RaidManager`に`raidRuntimeRoot`追加、`EnemySpawnManager`に`enemiesRoot`追加。敵スポーンはEnemies以下、LootリセットはLoot以下を参照。未設定時はフォールバックあり）
- スタミナ・飢餓渇き連動ダメージ実装済み（`PlayerState`にdamageCooldown方式の小数ダメージ蓄積、HUN=0で0.5/秒・THI=0で1/秒・両方0で2倍。`staminaDecayRate`/`staminaRecoveryRate`をInspector公開。`PlayerMove`はstamina=0でLeftShift無効化）

## 確認リスト（未対応）
- [x] Curious状態の追加（敵AI）
- [x] `RaidRuntimeRoot` の導入
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
Patrol → curiousRange内にPlayer → Curious → curiousPoint確認→3秒待機→curiousOriginPointに帰還 → Patrol
Curious/Patrol → CanSeePlayer → Chase → Attack
Patrol/Curious → 被弾 → Alert（20秒）→ Patrol
Chase → 見失う → Search（20秒）→ Alert → Patrol
Attack → 視線切れ → Search
```

## 開発ツール候補（将来検討）
- **Blender連携**：ClaudeでBlenderを制御して簡単な3Dオブジェクトを作成できる（リスナー情報・Day9配信）
- **AI画面判断**：Gemini Liveのような動画リアルタイム判断機能が実用化されれば、ゲームの挙動をAIが直接言語化してくれる可能性がある（Day9配信・配信者発言）

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