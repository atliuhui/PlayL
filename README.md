# PlayL - PlayList

## layout

```mermaid
graph LR;
  standard_lrc[LRC] .-> Admin;
  Admin --> Assets;
  Assets --> Agent;
  style standard_lrc fill:#cfe2ff,color:#0a58ca,stroke:#9ec5fe;
```

## spec

- content
- texts
- line
- line-label (alias: label)
- line-text (alias: text)
- label-content
- label-content-key (alias: label-key)
- label-content-value (alias: label-value)

```lrc
[ve:1]
[al:追梦痴子心]
[ti:骊歌]
[au:赵亮]
[ar:GALA]
[length:2:27]
[00:11.160]当这一切都结束 你是否失落
[00:21.060]当我随烟云消散 谁为我难过
[00:31.080]没有不散的伴侣 你要走下去
[00:41.000]没有不终的旋律 但我会继续
[00:50.870]倘若有天想起我 你蓦然寂寞
[01:00.740]人生是一场错过 愿你别蹉跎
[01:20.540]当这一切已结束 请不要失落
[01:30.470]我将随烟云消散 别为我难过
[01:40.430]千言万语不必说 只有一首歌
[01:50.260]都知欢聚最难得 难奈别离多
[02:00.110]都知欢聚最难得 难奈别离多
```

## todos

### admin

- [x] 检查Lyrics内容格式
- [x] 创建Lyrics内容，同时更新索引
- [x] 更新Lyrics内容，同时更新索引
- [x] 更新Lyrics索引时，去掉Label
- [x] 获取assets-lyrics的目录索引
- [x] 下载lyrics内容

### agent

- [x] 检索lyrics索引，根据关键词
- [x] 检索lyrics索引，根据字段
- [x] 获取assets-lyrics的目录索引
- [x] 下载lyrics内容
