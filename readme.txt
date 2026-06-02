<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">
    <title>文档中心 - 多格式文件预览</title>
    <!-- 引入 jQuery, Vue.js, PDF.js 核心库及缩放插件(使用 Viewer.js 辅助但自定义缩放) -->
    <script src="https://cdn.jsdelivr.net/npm/jquery@3.7.1/dist/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/vue@2.7.16/dist/vue.js"></script>
    <!-- PDF.js 核心库和 worker -->
    <script src="https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.min.js"></script>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: #f0f2f5;
            height: 100vh;
            overflow: hidden;
        }

        /* 整体布局 左右结构 */
        .app-container {
            display: flex;
            height: 100vh;
            width: 100%;
        }

        /* 左侧文件列表 */
        .file-sidebar {
            width: 280px;
            background: white;
            border-right: 1px solid #e4e7ed;
            display: flex;
            flex-direction: column;
            box-shadow: 2px 0 12px rgba(0,0,0,0.05);
            z-index: 10;
            overflow-y: auto;
        }

        .sidebar-header {
            padding: 20px 16px;
            background: #fafbfc;
            border-bottom: 1px solid #eaeef2;
        }

        .sidebar-header h3 {
            font-size: 18px;
            font-weight: 600;
            color: #2c3e50;
        }

        .file-list {
            flex: 1;
            list-style: none;
            padding: 8px 0;
        }

        .file-item {
            display: flex;
            align-items: center;
            padding: 12px 20px;
            margin: 2px 8px;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.2s;
            background: white;
        }

        .file-item:hover {
            background: #ecf5ff;
        }

        .file-item.active {
            background: #e6f7ff;
            border-left: 4px solid #1890ff;
            box-shadow: 0 2px 6px rgba(0,0,0,0.05);
        }

        .file-icon {
            font-size: 24px;
            margin-right: 12px;
            width: 32px;
            text-align: center;
        }

        .file-info {
            flex: 1;
            overflow: hidden;
        }

        .file-name {
            font-size: 14px;
            font-weight: 500;
            color: #1f2f3d;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
        }

        .file-type {
            font-size: 12px;
            color: #909399;
            margin-top: 4px;
        }

        /* 右侧内容区域 */
        .content-viewer {
            flex: 1;
            display: flex;
            flex-direction: column;
            background: #e9ecef;
            overflow: hidden;
        }

        /* 工具栏: 切换文件前后 + 缩放/翻页(上下文相关) */
        .toolbar {
            background: white;
            padding: 10px 20px;
            border-bottom: 1px solid #dee2e6;
            display: flex;
            align-items: center;
            gap: 16px;
            flex-wrap: wrap;
            box-shadow: 0 1px 4px rgba(0,0,0,0.05);
        }

        .nav-buttons {
            display: flex;
            gap: 10px;
        }

        .nav-btn {
            background: #f8f9fa;
            border: 1px solid #ced4da;
            padding: 6px 16px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 14px;
            transition: 0.2s;
        }

        .nav-btn:hover {
            background: #e9ecef;
        }

        .nav-btn:active {
            transform: scale(0.97);
        }

        .page-controls {
            display: flex;
            align-items: center;
            gap: 12px;
            background: #f8f9fa;
            padding: 4px 12px;
            border-radius: 24px;
            border: 1px solid #e2e6ea;
        }

        .page-controls button {
            background: none;
            border: none;
            font-size: 18px;
            cursor: pointer;
            width: 28px;
            height: 28px;
            border-radius: 50%;
            display: inline-flex;
            align-items: center;
            justify-content: center;
        }

        .page-controls button:hover {
            background: #dee2e6;
        }

        .page-info {
            font-size: 14px;
            min-width: 80px;
            text-align: center;
        }

        .zoom-controls {
            display: flex;
            align-items: center;
            gap: 8px;
            background: #f8f9fa;
            padding: 4px 12px;
            border-radius: 24px;
            border: 1px solid #e2e6ea;
        }

        .zoom-btn {
            background: white;
            border: 1px solid #ced4da;
            border-radius: 4px;
            width: 28px;
            height: 28px;
            cursor: pointer;
            font-weight: bold;
        }

        .zoom-level {
            font-size: 13px;
            min-width: 50px;
            text-align: center;
        }

        .separator {
            width: 1px;
            height: 24px;
            background: #dee2e6;
            margin: 0 4px;
        }

        /* 文件渲染区域 (滚动) */
        .render-area {
            flex: 1;
            overflow-y: auto;
            padding: 24px;
            background: #f1f3f5;
            display: flex;
            justify-content: center;
            align-items: flex-start;
            position: relative;
        }

        /* 内容包裹器 */
        .content-wrapper {
            max-width: 100%;
            background: white;
            border-radius: 12px;
            box-shadow: 0 8px 20px rgba(0,0,0,0.1);
            padding: 20px;
            transition: all 0.2s;
            display: inline-block;
            min-width: 60%;
        }

        /* 图片样式 + 缩放支持 */
        .image-view img {
            display: block;
            max-width: 100%;
            height: auto;
            transition: transform 0.2s;
            cursor: zoom-in;
        }

        /* 文本样式 */
        .text-view {
            white-space: pre-wrap;
            word-break: break-word;
            font-family: 'Consolas', monospace;
            font-size: 14px;
            line-height: 1.6;
            max-height: 70vh;
            overflow: auto;
        }

        /* pdf canvas 容器 */
        .pdf-view {
            display: flex;
            flex-direction: column;
            align-items: center;
        }

        .pdf-canvas {
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            margin-bottom: 12px;
            background: white;
            transition: transform 0.1s;
        }

        .loading-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(255,255,240,0.8);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 20;
            backdrop-filter: blur(2px);
        }

        .spinner {
            width: 48px;
            height: 48px;
            border: 4px solid #ddd;
            border-top: 4px solid #3498db;
            border-radius: 50%;
            animation: spin 0.8s linear infinite;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        .loading-text {
            margin-left: 12px;
            font-size: 16px;
            color: #2c3e50;
        }

        /* pdf 滚动内部分页指示 */
        .page-separator {
            text-align: center;
            margin: 12px 0;
            color: #6c757d;
        }
    </style>
</head>
<body>
<div id="app">
    <div class="app-container">
        <!-- 左侧文件列表区域 -->
        <div class="file-sidebar">
            <div class="sidebar-header">
                <h3>📁 文档列表</h3>
            </div>
            <ul class="file-list">
                <li v-for="file in fileList" :key="file.id" 
                    class="file-item" 
                    :class="{ active: currentFile && currentFile.id === file.id }"
                    @click="switchFile(file)">
                    <div class="file-icon">
                        <span v-if="file.type === 'pdf'">📄</span>
                        <span v-else-if="file.type === 'image'">🖼️</span>
                        <span v-else-if="file.type === 'text'">📃</span>
                        <span v-else>📎</span>
                    </div>
                    <div class="file-info">
                        <div class="file-name">{{ file.name }}</div>
                        <div class="file-type">{{ file.type.toUpperCase() }}</div>
                    </div>
                </li>
            </ul>
        </div>

        <!-- 右侧展示区域 -->
        <div class="content-viewer">
            <div class="toolbar">
                <div class="nav-buttons">
                    <button class="nav-btn" @click="prevFile" :disabled="!currentFile || fileIndex === 0">◀ 上一个</button>
                    <button class="nav-btn" @click="nextFile" :disabled="!currentFile || fileIndex === fileList.length-1">下一个 ▶</button>
                </div>
                <div class="separator"></div>
                <!-- PDF专用翻页控件 (仅当pdf且loaded) -->
                <div class="page-controls" v-if="currentFile && currentFile.type === 'pdf' && pdfDoc">
                    <button @click="prevPage" :disabled="currentPage <= 1">◀</button>
                    <span class="page-info">第 {{ currentPage }} / {{ totalPages }} 页</span>
                    <button @click="nextPage" :disabled="currentPage >= totalPages">▶</button>
                </div>
                <!-- 缩放控件 (pdf和图片支持缩放) -->
                <div class="zoom-controls" v-if="currentFile && (currentFile.type === 'pdf' || currentFile.type === 'image')">
                    <button class="zoom-btn" @click="zoomOut">-</button>
                    <span class="zoom-level">{{ Math.round(zoomLevel * 100) }}%</span>
                    <button class="zoom-btn" @click="zoomIn">+</button>
                    <button class="zoom-btn" @click="resetZoom" style="margin-left:4px;">重置</button>
                </div>
                <div style="flex:1"></div>
                <div style="font-size:13px; color:#6c757d;" v-if="currentFile">{{ currentFile.name }}</div>
            </div>

            <!-- 内容渲染区域，包含loading -->
            <div class="render-area" ref="renderArea">
                <!-- loading 遮罩 -->
                <div v-if="loading" class="loading-overlay">
                    <div class="spinner"></div>
                    <div class="loading-text">加载文件中...</div>
                </div>
                <!-- 实际内容容器 -->
                <div class="content-wrapper" v-show="!loading" ref="contentWrapper">
                    <!-- PDF 渲染容器 -->
                    <div v-if="currentFile && currentFile.type === 'pdf'" class="pdf-view" ref="pdfContainer">
                        <canvas v-for="(pageNum, idx) in renderedPages" :key="pageNum" 
                                :id="'pdf-canvas-'+pageNum" 
                                class="pdf-canvas"
                                :style="{ transform: `scale(${zoomLevel})`, transformOrigin: 'top center' }"></canvas>
                        <div v-if="totalPages > 1" class="page-separator">—— 第 {{ currentPage }} / {{ totalPages }} 页 ——</div>
                    </div>
                    <!-- 图片显示 -->
                    <div v-else-if="currentFile && currentFile.type === 'image'" class="image-view">
                        <img :src="imageSrc" alt="预览图片" ref="previewImage" :style="{ transform: `scale(${zoomLevel})`, transformOrigin: 'top left', cursor: 'zoom-in' }" @click="toggleImageZoom"/>
                    </div>
                    <!-- 纯文本显示 -->
                    <div v-else-if="currentFile && currentFile.type === 'text'" class="text-view">
                        <pre>{{ textContent }}</pre>
                    </div>
                    <div v-else-if="currentFile" class="text-view" style="color:gray;">不支持预览该格式</div>
                    <div v-else class="text-view" style="color:gray;">请从左侧选择文件</div>
                </div>
            </div>
        </div>
    </div>
</div>

<script>
    // PDF.js worker 配置 (CDN)
    pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.worker.min.js';

    // 模拟后端文件数据接口 (通过ajax获取文件列表及文件内容)
    // 由于需要演示，构造模拟 REST API 实现 (实际可用真实ajax替换，此处模拟数据便于展示)
    // 为了展示完整功能，构建模拟的后端数据，包含 pdf, 图片, 文本三种类型。
    // 实际环境中换成真实接口即可。
    const MOCK_API = {
        // 获取文件列表
        getFileList() {
            return new Promise((resolve) => {
                setTimeout(() => {
                    resolve([
                        { id: 1, name: '示例PDF文档.pdf', type: 'pdf', fileKey: 'sample.pdf' },
                        { id: 2, name: '风景图片.jpg', type: 'image', fileKey: 'landscape.jpg' },
                        { id: 3, name: '说明文档.txt', type: 'text', fileKey: 'readme.txt' },
                        { id: 4, name: 'Vue.js 指南.pdf', type: 'pdf', fileKey: 'vue_guide.pdf' },
                        { id: 5, name: 'logo图片.png', type: 'image', fileKey: 'logo.png' }
                    ]);
                }, 200);
            });
        },
        // 获取文件二进制或文本内容 (根据类型返回不同格式)
        getFileData(fileKey, type) {
            return new Promise((resolve, reject) => {
                setTimeout(async () => {
                    if (type === 'pdf') {
                        // 模拟一个简单PDF生成（为了避免外部真实pdf资源，采用动态生成一个含文字的示例PDF，或使用base64示例，为了确保可预览，利用pdf-lib? 不能增加复杂依赖，直接使用已有的示例pdf链接不符合跨域? 更好的模拟：使用基于文本生成一个简单pdf数据uri? 太复杂，我们用现成的在线示例pdf（合法），
                        // 但我们为了演示完全自包含，构建一个简单pdf Blob (用jsPDF? 会增加体积，不过我们可以使用外部已知 pdf 样例URL，但为保险使用 CDN pdf 测试文件，保证渲染成功)
                        // 鉴于演示环境中最好使用可靠测试pdf，使用一个现有的测试pdf (来自网络示例，仅用于展示功能) 
                        // 为了防止网络失效，使用生成器或base64? 简单方式: 加载外部合法pdf。存在跨域? pdf.js支持。选择: "https://raw.githubusercontent.com/mozilla/pdf.js/ba9f3c4b/examples/learning/helloworld.pdf" 但raw有时受限。
                        // 更稳健: 创建动态生成简单pdf的blob？太复杂。最好直接使用浏览器支持的样例: 使用'./sample.pdf' 不存在，所以更优雅: 通过fetch获取一个小pdf文件，利用现有 CDN pdf。
                        // 实际项目文件是后端提供，这里模拟成功获取arraybuffer。构建一个简短的pdf buffer 不可能手写。使用测试地址 https://cdn.mozilla.net/pdfjs/helloworld.pdf 可靠
                        const demoPdfUrl = 'https://cdn.mozilla.net/pdfjs/helloworld.pdf';
                        try {
                            const response = await fetch(demoPdfUrl);
                            if (!response.ok) throw new Error('PDF fetch failed');
                            const arrayBuffer = await response.arrayBuffer();
                            resolve(arrayBuffer);
                        } catch (err) {
                            // fallback: 生成一个简单的文本pdf提示无法加载正式示例，其实提示错误更好
                            console.warn("使用备用pdf生成简单提示消息");
                            // 使用一个简单伪造的消息, pdf不能伪造，尝试再用另一个url
                            const fallbackUrl = 'https://pdfobject.com/pdf/sample.pdf';
                            const res2 = await fetch(fallbackUrl);
                            if (res2.ok) resolve(await res2.arrayBuffer());
                            else reject(new Error('无法加载PDF示例'));
                        }
                    } else if (type === 'image') {
                        // 模拟不同图片，随机生成不同图片占位 / 为了展示真实图片，使用unsplash 占位或者data:image
                        let imgUrl = '';
                        if (fileKey === 'landscape.jpg') {
                            imgUrl = 'https://picsum.photos/id/1015/800/600';  // 山水
                        } else if (fileKey === 'logo.png') {
                            imgUrl = 'https://picsum.photos/id/1/400/400';
                        } else {
                            imgUrl = 'https://picsum.photos/600/400?random=1';
                        }
                        // 转blob其实直接用url渲染更简单，直接返回url地址
                        resolve(imgUrl);
                    } else if (type === 'text') {
                        // 模拟文本文档内容
                        let content = "这是一个示例文本文档。\n欢迎使用文档预览系统。\n支持缩放，左侧列表。\n第二行内容用于测试滚动效果。\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
                        if (fileKey === 'readme.txt') {
                            content = "说明文档：\n左侧文件列表包含了PDF，图片和文本文档。\n右侧预览区域支持PDF.js渲染，支持上下翻页，图片和PDF支持缩放。\n通过工具栏前后切换文件。\n文件加载均有loading效果。";
                        }
                        resolve(content);
                    } else {
                        reject('不支持的类型');
                    }
                }, 400); // 模拟网络延迟
            });
        }
    };

    new Vue({
        el: '#app',
        data: {
            fileList: [],          // 文件列表
            currentFile: null,     // 当前选中文件
            loading: false,
            // PDF 相关
            pdfDoc: null,
            currentPage: 1,
            totalPages: 0,
            renderedPages: [],     // 当前渲染页码（单页模式，但允许滚动单页）
            // 图片缩放
            zoomLevel: 1.0,
            imageSrc: null,
            textContent: '',
            // 缓存数据避免重复ajax
            fileDataCache: new Map()
        },
        computed: {
            fileIndex() {
                if (!this.currentFile) return -1;
                return this.fileList.findIndex(f => f.id === this.currentFile.id);
            }
        },
        watch: {
            currentFile(newFile, oldFile) {
                if (newFile && (!oldFile || oldFile.id !== newFile.id)) {
                    this.loadFileContent();
                }
            },
            // 监听缩放对图片实时应用(图片已经绑定style, pdf重新渲染canvas要动态scale, 直接通过样式)
            // pdf缩放我们通过canvas的transform scale实现，不需要重绘内容，但为了保证清晰，不用重新渲染直接css scale。
            // 但pdf的缩放会出现模糊？为了高质量可以考虑重新渲染，但这里为了演示简单使用css缩放即可，用户也接受。
            zoomLevel() {
                if (this.currentFile && this.currentFile.type === 'image' && this.$refs.previewImage) {
                    // 样式已绑定无需额外操作
                }
                // pdf scale通过绑定的style生效
            }
        },
        mounted() {
            this.init();
        },
        methods: {
            async init() {
                this.loading = true;
                try {
                    const list = await MOCK_API.getFileList();
                    this.fileList = list;
                    if (list.length > 0) {
                        this.currentFile = list[0];
                    }
                } catch (e) {
                    console.error(e);
                    alert("文件列表加载失败");
                } finally {
                    this.loading = false;
                }
            },
            async switchFile(file) {
                if (this.currentFile && this.currentFile.id === file.id) return;
                this.currentFile = file;
            },
            async loadFileContent() {
                if (!this.currentFile) return;
                // 重置视图状态
                this.resetViewState();
                this.loading = true;
                const file = this.currentFile;
                const cacheKey = `${file.id}_${file.type}`;
                if (this.fileDataCache.has(cacheKey)) {
                    const cached = this.fileDataCache.get(cacheKey);
                    this.applyFileData(cached);
                    this.loading = false;
                    return;
                }
                try {
                    const data = await MOCK_API.getFileData(file.fileKey, file.type);
                    this.fileDataCache.set(cacheKey, data);
                    this.applyFileData(data);
                } catch (err) {
                    console.error(err);
                    alert('文件加载失败');
                } finally {
                    this.loading = false;
                }
            },
            applyFileData(data) {
                const file = this.currentFile;
                if (file.type === 'pdf') {
                    this.loadPdf(data);
                } else if (file.type === 'image') {
                    this.imageSrc = data;   // data为url或者blob url
                    // 重置图片缩放
                    this.zoomLevel = 1.0;
                } else if (file.type === 'text') {
                    this.textContent = data;
                }
            },
            resetViewState() {
                // 清除pdf相关
                if (this.pdfDoc) {
                    this.pdfDoc.destroy?.();
                    this.pdfDoc = null;
                }
                this.totalPages = 0;
                this.currentPage = 1;
                this.renderedPages = [];
                this.imageSrc = null;
                this.textContent = '';
                this.zoomLevel = 1.0;
                // 清空canvas区域
                const container = this.$refs.pdfContainer;
                if (container) {
                    // 不操作，vnode 重新渲染
                }
            },
            async loadPdf(arrayBuffer) {
                try {
                    const pdf = await pdfjsLib.getDocument({ data: arrayBuffer }).promise;
                    this.pdfDoc = pdf;
                    this.totalPages = pdf.numPages;
                    this.currentPage = 1;
                    this.zoomLevel = 1.0;
                    await this.renderPage(this.currentPage);
                } catch (err) {
                    console.error('PDF解析错误', err);
                    alert('PDF加载失败');
                }
            },
            async renderPage(pageNumber) {
                if (!this.pdfDoc) return;
                // 仅渲染当前页，为了简单且支持上下滚动显示单页，保留单页渲染模式（但用户期望上下滚动对同一个文件进行翻页 - 工具栏翻页）
                // 满足: 右侧通过翻页按钮上下进行翻页。所以仅展示当前页
                const page = await this.pdfDoc.getPage(pageNumber);
                const viewport = page.getViewport({ scale: 1.5 }); // 基础渲染比例适中，缩放通过css完成提高清晰
                const canvasId = `pdf-canvas-${pageNumber}`;
                // 等待dom更新后渲染
                await this.$nextTick();
                let canvas = document.getElementById(canvasId);
                if (!canvas) {
                    // 动态创建，但因为是v-for已经存在，需要更新
                    this.renderedPages = [pageNumber];
                    await this.$nextTick();
                    canvas = document.getElementById(canvasId);
                }
                if (!canvas) return;
                const context = canvas.getContext('2d');
                canvas.width = viewport.width;
                canvas.height = viewport.height;
                const renderContext = {
                    canvasContext: context,
                    viewport: viewport,
                };
                await page.render(renderContext).promise;
                // 强制显示当前页码的canvas列表
                this.renderedPages = [pageNumber];
            },
            async nextPage() {
                if (!this.pdfDoc || this.currentPage >= this.totalPages) return;
                this.currentPage++;
                await this.renderPage(this.currentPage);
            },
            async prevPage() {
                if (!this.pdfDoc || this.currentPage <= 1) return;
                this.currentPage--;
                await this.renderPage(this.currentPage);
            },
            prevFile() {
                if (this.fileIndex > 0) {
                    this.switchFile(this.fileList[this.fileIndex - 1]);
                }
            },
            nextFile() {
                if (this.fileIndex < this.fileList.length - 1) {
                    this.switchFile(this.fileList[this.fileIndex + 1]);
                }
            },
            zoomIn() {
                if (this.currentFile && (this.currentFile.type === 'pdf' || this.currentFile.type === 'image')) {
                    this.zoomLevel = Math.min(this.zoomLevel + 0.2, 3.0);
                }
            },
            zoomOut() {
                if (this.currentFile && (this.currentFile.type === 'pdf' || this.currentFile.type === 'image')) {
                    this.zoomLevel = Math.max(this.zoomLevel - 0.2, 0.4);
                }
            },
            resetZoom() {
                this.zoomLevel = 1.0;
            },
            toggleImageZoom() {
                // 点击图片快捷方式
                if (this.zoomLevel === 1.0) this.zoomLevel = 1.8;
                else this.zoomLevel = 1.0;
            }
        }
    });
</script>
</body>
</html>
