# VostokLike 開発引継ぎ

## 開発者について
- コードを1行も書けない完全な素人・ゲーム開発初挑戦
- Inspector操作はオブジェクト名から丁寧に順を追って説明する
- 「なんかおかしい」という曖昧な報告でも原因を推測して対応する
- あらゆる可能性（コード・Inspector設定・Unity仕様・ジンバルロックなど）を視野に入れる

## ドキュメントフォルダ

### 文字起こし・要約ファイル
`C:\Users\user\projects\docs\STREAMINGDocs\`
- `transcript_dayXX.md` → 文字起こし原文
- `Summaried_transcript_dayXX.md` → 要約済み

### ゲーム開発ドキュメント
- `C:\Users\user\projects\docs\DesignDocs\` → 設計資料
- `C:\Users\user\projects\docs\IdeaDocs\` → アイデアメモ

### その他
- `C:\Users\user\projects\docs\` 配下に全ドキュメントを統一
- `C:\Users\user\My project (1)\` はUnityプロジェクトのみ

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
- `PlayerState.cs`：HP・hunger・thirst・stamina管理、死亡/リスポーン
- `Inventory.cs`：アイテム管理、グリッドUI、justOpenedフラグでFキー2重検知防止
- `BoxContainer.cs`：BoxGridのUI・アイテム操作、isOpenフラグ、CloseBoxメソッド
- `EquipmentSlot.cs`：装備スロット管理
- `EnemyAI.cs`：敵AI（Patrol/Curious/Alert/Search/Chase/Attack）
- `Gun.cs`：射撃・リロード・カメラシェイク
- `ExitZone.cs`：出撃・帰還・LootContainerインタラクト
- `LootContainer.cs`：ルートコンテナ（ScriptableObject方式、開閉ロジック実装済み）
- `SpawnManager.cs`：プレイヤースポーン管理
- `EnemySpawnManager.cs`：敵スポーン管理
- `StorageContainer.cs`：拠点保管コンテナ（永続Dictionary管理、BoxGrid UI流用、gridWidth=4×gridHeight=4）

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
- スタミナ減少条件：LeftShift+WASD入力中のみ減少（移動なしのLeftShift単体では減らない）。ダッシュ中は渇きも`staminaDecayRate×0.05/秒`で追加減少
- Curious状態実装済み（`EnemyAI`：curiousRange=40内にPlayerが入るとcuriousPointに移動→3秒待機→curiousOriginPointに帰還→Patrol。待機中CanSeePlayerでChaseに移行。`curiousRange`/`curiousWaitTime`をInspector公開）
- LootContainerのY座標統一済み（シーンファイル直接編集、Loot以下の全9コンテナをY=2221.2131に統一）
- 拠点保管コンテナ実装済み（`StorageContainer.cs`新規追加。Fキーでインタラクト、タグ「StorageContainer」。`BoxContainer.OpenStorage`でBoxGrid UIを流用。`AddToBox`で`currentStorageContainer.AddToContents`に双方向同期。ResetRaidWorldではリセットされない）
- スタック数保持修正済み（PlayerGrid→BoxGridおよびBoxGrid→PlayerGrid両方向で、実際のスタック数を正しく転送するよう修正。`MoveToBoxItem`は`specificItem.amount`、`MoveToPlayer`ローカル関数は`boxContents[itemName]`を使用）
- Curious発動に視線チェック追加済み（`HasLineOfSightToPlayer()`でRaycastによる壁チェック。FOV角度なし・距離curiousRange以内かつ遮蔽なしの場合のみCurious発動）
- 帰還時ステータス引き継ぎ実装済み（`PlayerState.ResumeAfterRaid()`追加。帰還時はHP/HUN/THI/STAをリセットせず継続、カーソルとタイムスケールのみ復元）
- 装備スロットForceUnequip修正済み（`RaidManager.equipmentSlots`をInspector直接参照配列に変更。死亡時に確実にForceUnequip呼び出し）
- PlayerGridクリックで武器装備実装済み（`Inventory.UpdateInventoryUI`のButtonリスナーでWeaponカテゴリチェック追加。`EquipmentSlot.EquipFromInventory`新規追加）
- BoxGridサブセルのインタラクト実装済み（`BoxContainer.CreateCell`でButton/EventTrigger/DraggableItemを全セルにアタッチ。テキスト表示はメインセル(dx==0&&dy==0)のみ）
- ダメージビネットエフェクト実装済み（被弾時に画面外周が赤くフェードイン→1秒でフェードアウト。死亡時は即非表示）
- マズルフラッシュ改善・硝煙追加済み（`ConfigureParticles()`でコード制御。MuzzleFlash：Cone形状・白〜薄黄・Additive。SmokeEffect：グレー・上方漂流・ColorOverLifetimeフェード）
- 銃声SE追加済み（`Gun.cs`・`EnemyAI.cs`両方に`AudioSource.PlayOneShot`方式で実装。Inspectorで`gunShotSound`設定）
- 建物生成Editorスクリプト実装済み（`Assets/Editor/GenerateHouses.cs`。VostokLike→Generate Housesで3軒生成。各軒：ドア2・窓4・MeshCollider・Static付き。NavMeshベイクは手動）
- アイテムドロップ実装済み（インベントリ開いた状態でCtrl+クリックで即ドロップ。スタックごと一括ドロップ。`Inventory.DropItem()`）
- アイテム掴み・設置実装済み（`ItemGrabber.cs`新規追加。Gキーで掴む→左クリックで設置・Gキー再押しでその場ドロップ。`IsHolding`フラグで`Gun.cs`の誤射を抑制）
- 時間システム実装済み（`TimeManager.cs`新規追加。gameTimeScale=60で現実1分=ゲーム内1時間。Directional Lightを時刻で回転。6時=日の出0度・12時=90度・18時=日没180度。時刻UIをHH:MM形式で表示）
- 焚火インタラクト実装済み（`Campfire.cs`新規追加。タグ「Campfire」。Fキーで`SleepManager.Sleep()`を呼び出す。`ExitZone.cs`にCampfireケース追加）
- 睡眠処理実装済み（`SleepManager.cs`。blackPanel.SetActive方式。黒パネル表示1秒→時刻8時間進める→hpMax/staminaMax+20%・hunger/thirst全回復→黒パネル非表示1秒。`SleepManager.IsSleeping`フラグでPlayerMove/PlayerState/Inventoryのガード）
- 睡眠バフ消失実装済み（バフ適用前に`hpMaxBase`/`staminaMaxBase`を記録。起床からゲーム内12時間後にhpMax/staminaMaxを基準値に戻してhpをClamp。`sleepBuffActive=false`で再眠可能に）
- 弾数HUD表示実装済み（`HUDManager.ammoText`追加。`Gun.Equip/Unequip/Shoot/Reload`完了時にUpdateAmmo/HideAmmo呼び出し。0発で赤文字・未装備時非表示）
- 死体コンテナ実装済み（`Enemy.OnDeath()`でDestroy→停止切替。EnemyAI/NavMeshAgent/Animator無効化、タグをCorpseに変更。`LootContainer`をタグ制御でFキーインタラクト対応。`ContainerData_Corpse`：emptyChance35%、9x18mm(60%)/MedKit(15%)/BeefCan(20%)。レイドリセット時にEnemiesRoot以下ごと削除）
- 建物ドア引っかかり修正済み（`GenerateHouses.cs`でMeshCollider→BoxCollider変更・DoorW=2.0→2.4に拡張）
- BoxGridのTooltip残留バグ修正済み（`BoxContainer.PrimaryAction()`先頭で`tooltipText.gameObject.SetActive(false)`を呼ぶように修正）
- 武器グリッドセル残弾数表示実装済み（`AmmoDisplay.cs`新規追加。PlayerGrid・BoxGrid両方のWeaponセルにAddComponentで表示。`InventoryItem.ammo`（デフォルト0）で常時表示。装備・解除時にGun.currentAmmoと同期。`Inventory.GetAmmo()`でEquipmentSlotが装備前に残弾を取得する方式）
- マガジンシステムPhase1実装済み（`ItemData.Magazine`カテゴリ追加・maxAmmo/compatibleWeapon/compatibleBullet追加。`MagazineDropHandler.cs`新規：9x18mmをマガジンにD&Dで一括装填。`Gun.Reload()`をマガジン交換方式に変更（旧マガジン残弾保持してインベントリ返却）。マガジンなし・弾切れで射撃不可。`CreateMagazineAsset.cs`でPMPMagazineプレハブ生成）
- D&D中のDestroyImmediate crash修正済み（`Inventory.RemoveAmmoBatch()`追加：UpdateInventoryUI不呼び出しのバッチ削除。`Inventory.ScheduleUIUpdate()`追加：1フレーム遅延でUI更新。`MagazineDropHandler`でこれらを使用し、イベント処理中のRectTransform破壊を回避）
- セカンダリアクション実装済み（武器右クリック＝RemoveMagazine、マガジン左クリック＝Attach、マガジン右クリック＝Unload。PlayerGrid・BoxGrid・装備スロット全対応。`Gun.RemoveMagazineToInventory()`・`ReloadWith(InventoryItem)`追加。`Gun.Reload()`を`ReloadWithCoroutine()`に分離）
- BoxGridマガジンammo追跡実装済み（`BoxContainer.boxInventoryItems`追加。`AddMagazineToBox(name,ammo)`でammo付き登録。PlayerGrid↔BoxGrid移動でammo保持。BoxGridでもAmmoDisplay・MagazineDropHandler・右クリックUnload対応）
- ジャンプ実装済み（Spaceキー・`isGrounded`で接地判定・STA0時ジャンプ不可・ジャンプごとにSTA-10。`jumpHeight`はInspector公開）
- ステータスUI色変化実装済み（80%以上=緑#44FF2F・21〜79%=白・20%以下=赤。HP/STA/HUN/THIは`HUDManager.StatusColor()`で管理、MENは`PlayerState.Update()`で管理）
- BeefCan時間HP回復実装済み（`PlayerState.HealOverTime(10)`追加。0.5秒ごとに1HP×10回で5秒かけて回復。再使用時は`pendingHeal`に加算して継続。`ItemManager.PickupFood`から呼び出し）

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

## 新機能実装前ルール
1. 既存ゲームの実装例を調査する
2. 実装難易度を比較する
3. 現在の開発段階に最適な簡易版を優先提案する
4. 「今必要なのは完成度ではなくゲームループ」を判断基準にする

## 既知のバグ・教訓

### Directional Lightのジンバルロック（解決済み）
- 症状：昼夜サイクルで12時に太陽が分裂・点滅する
- 原因：Quaternion.EulerのX軸が90度付近でジンバルロックが発生
- 解決：Quaternion.AngleAxisを使用（`TimeManager.cs`）
- 発見方法：Game Time Scaleを3600に上げて高速観測

### UIの点滅バグ調査手順
1. blackPanelを非アクティブにして点滅が再現するか確認
2. コードが正常でもレンダリング側に原因がある場合がある
3. AdvanceTime後にDirectional Lightの角度が急変していないか確認

### 昼夜サイクルのデバッグ方法
- Game Time Scaleを大きくして高速観測すると異常を発見しやすい
- Directional LightはQuaternion.AngleAxisで設定する

## 開発ツール候補（将来検討）
- **Blender連携**：ClaudeでBlenderを制御して簡単な3Dオブジェクトを作成できる（リスナー情報・Day9配信）
- **AI画面判断**：Gemini Liveのような動画リアルタイム判断機能が実用化されれば、ゲームの挙動をAIが直接言語化してくれる可能性がある（Day9配信・配信者発言）

## バグ報告テンプレート
ブラウザのClaudeに以下の形式で報告してからVS Codeに投げること：

- 何をしたか（操作）
- 何が起きたか（症状）
- 何が起きると思っていたか（期待）
- 再現条件（毎回？特定条件のみ？）
- スクショ・動画（あれば）

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
## Language
Respond in Japanese (日本語で回答してください).

## Git Workflow
When the user requests a commit, proceed with staged files without asking for confirmation unless there are clearly unrelated changes. Always commit AND push when the user says 'commit' for this repo.

## Unity Development
- After making code changes, remind the user that Unity needs to recompile before testing.
- Verify code state thoroughly (Read the file, check related scripts for duplicates like duplicate ExitZone components) BEFORE making edits.
- For interaction bugs, check for duplicate MonoBehaviour scripts on the same GameObject as a common root cause.
