<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no">
    <title>文档中心 - 多格式文件预览（稳定版）</title>
    <script src="https://cdn.jsdelivr.net/npm/jquery@3.7.1/dist/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/vue@2.7.16/dist/vue.js"></script>
    <!-- PDF.js 核心库 - 使用稳定版本 -->
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

        .app-container {
            display: flex;
            height: 100vh;
            width: 100%;
        }

        .file-sidebar {
            width: 280px;
            background: white;
            border-right: 1px solid #e4e7ed;
            display: flex;
            flex-direction: column;
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

        .content-viewer {
            flex: 1;
            display: flex;
            flex-direction: column;
            background: #e9ecef;
            overflow: hidden;
        }

        .toolbar {
            background: white;
            padding: 10px 20px;
            border-bottom: 1px solid #dee2e6;
            display: flex;
            align-items: center;
            gap: 16px;
            flex-wrap: wrap;
        }

        .nav-btn {
            background: #f8f9fa;
            border: 1px solid #ced4da;
            padding: 6px 16px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 14px;
        }

        .nav-btn:hover:not(:disabled) {
            background: #e9ecef;
        }

        .nav-btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .page-controls, .zoom-controls {
            display: flex;
            align-items: center;
            gap: 8px;
            background: #f8f9fa;
            padding: 4px 12px;
            border-radius: 24px;
            border: 1px solid #e2e6ea;
        }

        .page-controls button, .zoom-btn {
            background: white;
            border: 1px solid #ced4da;
            border-radius: 4px;
            width: 28px;
            height: 28px;
            cursor: pointer;
        }

        .page-controls button:hover:not(:disabled), .zoom-btn:hover {
            background: #e9ecef;
        }

        .separator {
            width: 1px;
            height: 24px;
            background: #dee2e6;
        }

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

        .content-wrapper {
            background: white;
            border-radius: 12px;
            box-shadow: 0 8px 20px rgba(0,0,0,0.1);
            padding: 20px;
            display: inline-block;
            min-width: 200px;
            max-width: 100%;
        }

        .image-view {
            display: inline-block;
            text-align: center;
        }

        .image-view img {
            display: block;
            max-width: none;
            height: auto;
            transition: transform 0.2s ease;
            cursor: zoom-in;
            transform-origin: top left;
        }

        .text-view {
            white-space: pre-wrap;
            word-break: break-word;
            font-family: 'Consolas', monospace;
            font-size: 14px;
            line-height: 1.6;
            max-height: 70vh;
            overflow: auto;
        }

        .pdf-view {
            display: inline-block;
            text-align: center;
        }

        .pdf-canvas {
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            background: white;
        }

        .loading-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(255,255,240,0.9);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 20;
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
            to { transform: rotate(360deg); }
        }

        .loading-text {
            margin-left: 12px;
            font-size: 16px;
            color: #2c3e50;
        }

        .render-status {
            position: fixed;
            bottom: 20px;
            right: 20px;
            background: rgba(0,0,0,0.8);
            color: #4ecdc4;
            padding: 8px 16px;
            border-radius: 20px;
            font-size: 12px;
            font-family: monospace;
            z-index: 100;
            pointer-events: none;
            max-width: 300px;
        }

        .error-badge {
            background: #d63031;
            color: white;
            padding: 2px 8px;
            border-radius: 12px;
            font-size: 11px;
            margin-left: 8px;
        }
    </style>
</head>
<body>
<div id="app">
    <div class="app-container">
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
                    </div>
                    <div class="file-info">
                        <div class="file-name">{{ file.name }}</div>
                        <div class="file-type">{{ file.type.toUpperCase() }}</div>
                    </div>
                </li>
            </ul>
        </div>

        <div class="content-viewer">
            <div class="toolbar">
                <div class="nav-buttons">
                    <button class="nav-btn" @click="prevFile" :disabled="!currentFile || fileIndex === 0">◀ 上一个</button>
                    <button class="nav-btn" @click="nextFile" :disabled="!currentFile || fileIndex === fileList.length-1">下一个 ▶</button>
                </div>
                <div class="separator"></div>
                <div class="page-controls" v-if="currentFile && currentFile.type === 'pdf' && pdfDoc">
                    <button @click="prevPage" :disabled="currentPage <= 1">◀</button>
                    <span class="page-info">第 {{ currentPage }} / {{ totalPages }} 页</span>
                    <button @click="nextPage" :disabled="currentPage >= totalPages">▶</button>
                </div>
                <div class="zoom-controls" v-if="currentFile && (currentFile.type === 'pdf' || currentFile.type === 'image')">
                    <button class="zoom-btn" @click="zoomOut">-</button>
                    <span class="zoom-level">{{ Math.round(zoomLevel * 100) }}%</span>
                    <button class="zoom-btn" @click="zoomIn">+</button>
                    <button class="zoom-btn" @click="resetZoom">重置</button>
                </div>
                <div style="flex:1"></div>
                <div style="font-size:13px; color:#6c757d;">{{ currentFile ? currentFile.name : '请选择文件' }}</div>
            </div>

            <div class="render-area" ref="renderArea">
                <div v-if="loading" class="loading-overlay">
                    <div class="spinner"></div>
                    <div class="loading-text">加载文件中...</div>
                </div>
                <div class="content-wrapper" v-show="!loading" ref="contentWrapper">
                    <div v-if="currentFile && currentFile.type === 'pdf'" class="pdf-view">
                        <canvas id="pdf-canvas" class="pdf-canvas" 
                                :style="{ transform: `scale(${zoomLevel})`, transformOrigin: 'top left' }"></canvas>
                        <div v-if="totalPages > 1" style="text-align:center; margin-top:12px; color:#666;">
                            第 {{ currentPage }} / {{ totalPages }} 页
                        </div>
                    </div>
                    <div v-else-if="currentFile && currentFile.type === 'image'" class="image-view">
                        <img :src="imageSrc" ref="previewImage" 
                             :style="{ transform: `scale(${zoomLevel})`, transformOrigin: 'top left' }" 
                             @click="toggleImageZoom"
                             @load="onImageLoad" />
                    </div>
                    <div v-else-if="currentFile && currentFile.type === 'text'" class="text-view">
                        <pre>{{ textContent }}</pre>
                    </div>
                    <div v-else class="text-view" style="color:gray;">请从左侧选择文件</div>
                </div>
            </div>
        </div>
    </div>
    <div class="render-status" v-if="renderStatus" v-html="renderStatus"></div>
</div>

<script>
    // 配置 PDF.js worker
    pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.worker.min.js';
    
    // 禁用 PDF.js 的某些严格检查，提高容错性
    pdfjsLib.disableFontFace = false;
    
    // ==================== 辅助函数 ====================
    
    /**
     * 将 Base64 字符串转换为 Uint8Array
     */
    function base64ToUint8Array(base64) {
        try {
            // 移除可能的前缀
            let cleanBase64 = base64;
            if (base64.includes(',')) {
                cleanBase64 = base64.split(',')[1];
            }
            const binaryString = atob(cleanBase64);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }
            return bytes;
        } catch (e) {
            console.error('Base64转换失败:', e);
            throw new Error('Base64解码失败');
        }
    }
    
    /**
     * 将字符串转换为 Uint8Array（使用 TextEncoder）
     */
    function stringToUint8Array(str) {
        const encoder = new TextEncoder();
        return encoder.encode(str);
    }
    
    /**
     * 智能转换 PDF 数据为 Uint8Array
     * 支持：Uint8Array, ArrayBuffer, Base64字符串, 普通字符串
     */
    function normalizePdfData(data) {
        // 已经是 Uint8Array
        if (data instanceof Uint8Array) {
            return data;
        }
        // ArrayBuffer
        if (data instanceof ArrayBuffer) {
            return new Uint8Array(data);
        }
        // 字符串
        if (typeof data === 'string') {
            // 检查是否是有效的 PDF 文件头
            const trimmed = data.trim();
            // 如果是 Base64 编码（以 JVBER 开头或包含 PDF 特征）
            if (trimmed.startsWith('JVBER') || trimmed.substring(0, 20).includes('JVBER')) {
                try {
                    return base64ToUint8Array(trimmed);
                } catch (e) {
                    console.warn('Base64转换失败，尝试字符串转换');
                }
            }
            // 检查是否以 %PDF- 开头（原始 PDF 字符串）
            if (trimmed.startsWith('%PDF-')) {
                return stringToUint8Array(trimmed);
            }
            // 尝试作为 Base64 处理（带 data URL 前缀）
            if (trimmed.includes('base64,')) {
                const base64Part = trimmed.split('base64,')[1];
                if (base64Part) {
                    return base64ToUint8Array(base64Part);
                }
            }
            // 最后尝试直接字符串转换
            return stringToUint8Array(trimmed);
        }
        // 其他情况
        throw new Error('不支持的 PDF 数据格式');
    }
    
    /**
     * 验证 PDF 数据是否有效
     */
    function isValidPdfData(data) {
        if (!data || data.length < 8) return false;
        // PDF 文件头应该是 %PDF-
        const header = String.fromCharCode(data[0], data[1], data[2], data[3], data[4]);
        return header === '%PDF-';
    }
    
    // ==================== 模拟后端API（使用更稳定的PDF数据）====================
    
    // 生成一个简单但完全有效的 PDF（使用标准 PDF 格式）
    // 这个 PDF 不使用 Flate 压缩，避免解压错误
    const SIMPLE_PDF_BASE64 = 'JVBERi0xLjQKMSAwIG9iago8PCAvVHlwZSAvQ2F0YWxvZwogL1BhZ2VzIDIgMCBSCj4+CmVuZG9iagoyIDAgb2JqCjw8IC9UeXBlIC9QYWdlcwogL0tpZHMgWzMgMCBSXQogL0NvdW50IDEKPj4KZW5kb2JqCjMgMCBvYmoKPDwgL1R5cGUgL1BhZ2UKIC9QYXJlbnQgMiAwIFIKIC9SZXNvdXJjZXMgPDwKL0ZvbnQgPDwgL0YxIDQgMCBSID4+Ci9Qcm9jU2V0IFsvUERGIC9UZXh0XQo+PgovTWVkaWFCb3ggWzAgMCA2MTIgNzkyXQovQ29udGVudHMgNSAwIFIKPj4KZW5kb2JqCjQgMCBvYmoKPDwgL1R5cGUgL0ZvbnQKL1N1YnR5cGUgL1R5cGUxCi9CYXNlRm9udCAvSGVsdmV0aWNhCj4+CmVuZG9iago1IDAgb2JqCjw8IC9MZW5ndGggNzAgPj4Kc3RyZWFtCkJUCi9GMSAyNCBUZgoxMDAgNzAwIFRkCihQREYgUmVuZGVyaW5nIFN1Y2Nlc3NmdWwhIC0g5rKz5Y+R5oiQ5YqfKSBUagpFVAplbmRzdHJlYW0KZW5kb2JqCnhyZWYKMCA2CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAxMCAwMDAwMCBuIAowMDAwMDAwMDU5IDAwMDAwIG4gCjAwMDAwMDAxMTMgMDAwMDAgbiAKMDAwMDAwMDI0MiAwMDAwMCBuIAowMDAwMDAwMzA5IDAwMDAwIG4gCnRyYWlsZXIKPDwgL1NpemUgNiAvUm9vdCAxIDAgUiA+PgpzdGFydHhyZWYKNDI2CiUlRU9G';
    
    const CHINESE_PDF_BASE64 = 'JVBERi0xLjQKMSAwIG9iago8PCAvVHlwZSAvQ2F0YWxvZwogL1BhZ2VzIDIgMCBSCj4+CmVuZG9iagoyIDAgb2JqCjw8IC9UeXBlIC9QYWdlcwogL0tpZHMgWzMgMCBSXQogL0NvdW50IDEKPj4KZW5kb2JqCjMgMCBvYmoKPDwgL1R5cGUgL1BhZ2UKIC9QYXJlbnQgMiAwIFIKIC9SZXNvdXJjZXMgPDwKL0ZvbnQgPDwgL0YxIDQgMCBSID4+Cj4+Ci9NZWRpYUJveCBbMCAwIDYxMiA3OTJdCi9Db250ZW50cyA1IDAgUgo+PgplbmRvYmoKNCAwIG9iago8PCAvVHlwZSAvRm9udAovU3VidHlwZSAvVHlwZTEKL0Jhc2VGb250IC9IZWx2ZXRpY2EKPj4KZW5kb2JqCjUgMCBvYmoKPDwgL0xlbmd0aCAxMjAgPj4Kc3RyZWFtCkJUCi9GMSAyNCBUZgoxMDAgNzAwIFRkCijkuK3mlofnvJPlrZcgLSBQREYg5aaH5qC857uE5Y+R77yBKSBUagowIC0yMCBURCjmt7Hlip/lvIDlj5EpIFRqCjAgLTIwIFREKOaVsOaNruW5tuWQjuWPkeWkqikgVGoKMCAtMjAgVEQo5Y2V5bCG5aaH5qC85p2l5rqQ77yBKCBUagpFVAplbmRzdHJlYW0KZW5kb2JqCnhyZWYKMCA2CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAxMCAwMDAwMCBuIAowMDAwMDAwMDU5IDAwMDAwIG4gCjAwMDAwMDAxMTMgMDAwMDAgbiAKMDAwMDAwMDIyNiAwMDAwMCBuIAowMDAwMDAwMjkzIDAwMDAwIG4gCnRyYWlsZXIKPDwgL1NpemUgNiAvUm9vdCAxIDAgUiA+PgpzdGFydHhyZWYKNDUwCiUlRU9G';
    
    const MOCK_API = {
        getFileList() {
            return new Promise((resolve) => {
                setTimeout(() => {
                    resolve([
                        { id: 1, name: '测试PDF（英文）.pdf', type: 'pdf', fileKey: 'simple.pdf' },
                        { id: 2, name: '测试PDF（中文）.pdf', type: 'pdf', fileKey: 'chinese.pdf' },
                        { id: 3, name: '风景图片.jpg', type: 'image', fileKey: 'landscape.jpg' },
                        { id: 4, name: '说明文档.txt', type: 'text', fileKey: 'readme.txt' },
                        { id: 5, name: 'Logo图片.png', type: 'image', fileKey: 'logo.png' }
                    ]);
                }, 200);
            });
        },
        getFileData(fileKey, type) {
            return new Promise((resolve, reject) => {
                setTimeout(() => {
                    if (type === 'pdf') {
                        if (fileKey === 'simple.pdf') {
                            // 返回 Uint8Array 格式，避免字符串转换问题
                            resolve(base64ToUint8Array(SIMPLE_PDF_BASE64));
                        } else {
                            resolve(base64ToUint8Array(CHINESE_PDF_BASE64));
                        }
                    } else if (type === 'image') {
                        let imgUrl = '';
                        if (fileKey === 'landscape.jpg') {
                            imgUrl = 'https://picsum.photos/id/1015/800/600';
                        } else {
                            imgUrl = 'https://picsum.photos/id/1/400/400';
                        }
                        resolve(imgUrl);
                    } else if (type === 'text') {
                        let content = "这是一个示例文本文档。\n\n欢迎使用文档预览系统。\n\n功能特点：\n1. 支持 PDF 文件预览（使用 PDF.js）\n2. 支持图片预览和缩放\n3. 支持文本文件预览\n4. 左右文件切换\n5. PDF 上下翻页\n\n✨ 本次优化：\n- 修复了 Flate 解压错误\n- 使用标准 PDF 格式，避免压缩问题\n- 增强了数据格式转换的容错性";
                        resolve(content);
                    } else {
                        reject('不支持的类型');
                    }
                }, 300);
            });
        }
    };
    
    // ==================== Vue 应用 ====================
    new Vue({
        el: '#app',
        data: {
            fileList: [],
            currentFile: null,
            loading: false,
            renderStatus: '',
            // PDF 相关
            pdfDoc: null,
            currentPage: 1,
            totalPages: 0,
            // 图片相关
            zoomLevel: 1.0,
            imageSrc: null,
            textContent: '',
            imageNaturalWidth: 0,
            imageNaturalHeight: 0,
            // 缓存
            fileDataCache: new Map(),
            // 错误计数
            pdfErrorCount: 0
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
            }
        },
        mounted() {
            this.init();
        },
        methods: {
            async init() {
                this.loading = true;
                this.showStatus('正在加载文件列表...', 'info');
                try {
                    const list = await MOCK_API.getFileList();
                    this.fileList = list;
                    if (list.length > 0) {
                        this.currentFile = list[0];
                    }
                    this.showStatus('✅ 就绪', 'success');
                    setTimeout(() => this.clearStatus(), 2000);
                } catch (e) {
                    console.error(e);
                    this.showStatus('❌ 文件列表加载失败', 'error');
                } finally {
                    this.loading = false;
                }
            },
            showStatus(msg, type) {
                const color = type === 'error' ? '#d63031' : (type === 'success' ? '#00b894' : '#4ecdc4');
                this.renderStatus = `<span style="color:${color}">${msg}</span>`;
            },
            clearStatus() {
                if (this.renderStatus && !this.renderStatus.includes('❌')) {
                    setTimeout(() => { this.renderStatus = ''; }, 1000);
                }
            },
            async switchFile(file) {
                if (this.currentFile && this.currentFile.id === file.id) return;
                this.currentFile = file;
            },
            async loadFileContent() {
                if (!this.currentFile) return;
                this.resetViewState();
                this.loading = true;
                this.showStatus(`正在加载 ${this.currentFile.name}...`, 'info');
                
                const file = this.currentFile;
                const cacheKey = `${file.id}_${file.type}`;
                
                if (this.fileDataCache.has(cacheKey)) {
                    const cached = this.fileDataCache.get(cacheKey);
                    this.applyFileData(cached);
                    this.loading = false;
                    this.showStatus(`✅ 加载完成`, 'success');
                    this.clearStatus();
                    return;
                }
                
                try {
                    const data = await MOCK_API.getFileData(file.fileKey, file.type);
                    this.fileDataCache.set(cacheKey, data);
                    this.applyFileData(data);
                    this.showStatus(`✅ 加载完成`, 'success');
                    this.clearStatus();
                } catch (err) {
                    console.error('文件加载失败:', err);
                    this.showStatus(`❌ 加载失败: ${err.message}`, 'error');
                } finally {
                    this.loading = false;
                }
            },
            applyFileData(data) {
                const file = this.currentFile;
                if (file.type === 'pdf') {
                    this.loadPdf(data);
                } else if (file.type === 'image') {
                    this.imageSrc = data;
                    this.zoomLevel = 1.0;
                } else if (file.type === 'text') {
                    this.textContent = data;
                }
            },
            resetViewState() {
                if (this.pdfDoc) {
                    try { this.pdfDoc.destroy(); } catch(e) {}
                    this.pdfDoc = null;
                }
                this.totalPages = 0;
                this.currentPage = 1;
                this.imageSrc = null;
                this.textContent = '';
                this.zoomLevel = 1.0;
                this.pdfErrorCount = 0;
                
                // 清空 canvas
                const canvas = document.getElementById('pdf-canvas');
                if (canvas) {
                    const ctx = canvas.getContext('2d');
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                }
            },
            async loadPdf(data) {
                try {
                    // 智能转换 PDF 数据
                    let pdfData;
                    try {
                        pdfData = normalizePdfData(data);
                    } catch (normError) {
                        console.error('数据转换失败:', normError);
                        this.showStatus('❌ PDF 数据格式转换失败', 'error');
                        return;
                    }
                    
                    // 验证 PDF 数据有效性
                    if (!isValidPdfData(pdfData)) {
                        this.showStatus('❌ 无效的 PDF 数据', 'error');
                        return;
                    }
                    
                    this.showStatus('正在解析 PDF...', 'info');
                    
                    // PDF.js 配置 - 提高容错性
                    const loadingTask = pdfjsLib.getDocument({
                        data: pdfData,
                        useSystemFonts: true,
                        disableFontFace: false,
                        disableRange: true,      // 禁用范围请求，避免分块问题
                        disableStream: true,     // 禁用流式加载
                        disableAutoFetch: true,  // 禁用自动获取
                        cMapUrl: 'https://cdn.jsdelivr.net/npm/pdfjs-dist@2.16.105/cmaps/',
                        cMapPacked: true,
                        // 忽略一些错误
                        ignoreErrors: true,
                        stopAtErrors: false
                    });
                    
                    const pdf = await loadingTask.promise;
                    this.pdfDoc = pdf;
                    this.totalPages = pdf.numPages;
                    this.currentPage = 1;
                    this.zoomLevel = 1.0;
                    
                    this.showStatus(`📄 PDF加载成功，共 ${this.totalPages} 页`, 'success');
                    
                    await this.$nextTick();
                    await this.renderPage(1);
                    
                } catch (err) {
                    console.error('PDF加载错误详情:', err);
                    this.pdfErrorCount++;
                    
                    // 提供更友好的错误信息
                    let errorMsg = err.message || '未知错误';
                    if (errorMsg.includes('flate')) {
                        errorMsg = 'PDF 压缩流错误，文件可能已损坏';
                    } else if (errorMsg.includes('format')) {
                        errorMsg = 'PDF 格式错误';
                    }
                    
                    this.showStatus(`❌ PDF加载失败: ${errorMsg}`, 'error');
                    
                    // 在 canvas 上显示错误信息
                    this.showPdfError(errorMsg);
                }
            },
            async renderPage(pageNumber) {
                if (!this.pdfDoc) return;
                
                try {
                    this.showStatus(`正在渲染第 ${pageNumber} 页...`, 'info');
                    const page = await this.pdfDoc.getPage(pageNumber);
                    
                    // 获取容器宽度
                    const container = this.$refs.contentWrapper;
                    const containerWidth = container ? container.clientWidth - 40 : 700;
                    const originalViewport = page.getViewport({ scale: 1.0 });
                    
                    // 计算缩放，使页面适应容器宽度
                    let scale = containerWidth / originalViewport.width;
                    scale = Math.min(Math.max(scale, 0.5), 1.8);
                    scale = scale * this.zoomLevel;
                    
                    const viewport = page.getViewport({ scale: scale });
                    
                    await this.$nextTick();
                    const canvas = document.getElementById('pdf-canvas');
                    if (!canvas) return;
                    
                    const context = canvas.getContext('2d', { alpha: false });
                    
                    canvas.width = viewport.width;
                    canvas.height = viewport.height;
                    canvas.style.width = viewport.width + 'px';
                    canvas.style.height = viewport.height + 'px';
                    
                    // 填充白色背景
                    context.fillStyle = 'white';
                    context.fillRect(0, 0, canvas.width, canvas.height);
                    
                    const renderContext = {
                        canvasContext: context,
                        viewport: viewport,
                        background: 'white',
                        intent: 'display'
                    };
                    
                    await page.render(renderContext).promise;
                    this.showStatus(`✅ 第 ${pageNumber} 页渲染完成`, 'success');
                    this.clearStatus();
                    
                } catch (error) {
                    console.error('渲染页面失败:', error);
                    this.showStatus(`❌ 渲染失败: ${error.message}`, 'error');
                    this.showPdfError(error.message);
                }
            },
            showPdfError(message) {
                const canvas = document.getElementById('pdf-canvas');
                if (canvas) {
                    const ctx = canvas.getContext('2d');
                    ctx.fillStyle = '#fff5f5';
                    ctx.fillRect(0, 0, canvas.width || 600, canvas.height || 400);
                    ctx.fillStyle = '#d63031';
                    ctx.font = '14px monospace';
                    ctx.fillText('PDF 渲染错误: ' + message, 20, 50);
                    ctx.fillText('请尝试刷新或选择其他文件', 20, 80);
                }
            },
            async nextPage() {
                if (this.pdfDoc && this.currentPage < this.totalPages) {
                    this.currentPage++;
                    await this.renderPage(this.currentPage);
                }
            },
            async prevPage() {
                if (this.pdfDoc && this.currentPage > 1) {
                    this.currentPage--;
                    await this.renderPage(this.currentPage);
                }
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
                if (this.currentFile) {
                    this.zoomLevel = Math.min(this.zoomLevel + 0.2, 3.0);
                    if (this.currentFile.type === 'pdf' && this.pdfDoc) {
                        this.renderPage(this.currentPage);
                    } else if (this.currentFile.type === 'image') {
                        this.$nextTick(() => this.adjustContentWrapperForImage());
                    }
                }
            },
            zoomOut() {
                if (this.currentFile) {
                    this.zoomLevel = Math.max(this.zoomLevel - 0.2, 0.4);
                    if (this.currentFile.type === 'pdf' && this.pdfDoc) {
                        this.renderPage(this.currentPage);
                    } else if (this.currentFile.type === 'image') {
                        this.$nextTick(() => this.adjustContentWrapperForImage());
                    }
                }
            },
            resetZoom() {
                this.zoomLevel = 1.0;
                if (this.currentFile && this.currentFile.type === 'pdf' && this.pdfDoc) {
                    this.renderPage(this.currentPage);
                } else if (this.currentFile && this.currentFile.type === 'image') {
                    this.$nextTick(() => this.adjustContentWrapperForImage());
                }
            },
            toggleImageZoom() {
                if (this.zoomLevel === 1.0) {
                    this.zoomLevel = 1.8;
                } else {
                    this.zoomLevel = 1.0;
                }
                this.$nextTick(() => this.adjustContentWrapperForImage());
            },
            onImageLoad(event) {
                const img = event.target;
                this.imageNaturalWidth = img.naturalWidth;
                this.imageNaturalHeight = img.naturalHeight;
                this.adjustContentWrapperForImage();
            },
            adjustContentWrapperForImage() {
                const imgElement = this.$refs.previewImage;
                const wrapper = this.$refs.contentWrapper;
                if (!imgElement || !wrapper) return;
                
                const naturalWidth = this.imageNaturalWidth || imgElement.naturalWidth || 400;
                const naturalHeight = this.imageNaturalHeight || imgElement.naturalHeight || 300;
                const scaledWidth = naturalWidth * this.zoomLevel;
                const scaledHeight = naturalHeight * this.zoomLevel;
                
                wrapper.style.width = scaledWidth + 'px';
                wrapper.style.minWidth = scaledWidth + 'px';
                
                const imageViewDiv = imgElement.parentElement;
                if (imageViewDiv) {
                    imageViewDiv
