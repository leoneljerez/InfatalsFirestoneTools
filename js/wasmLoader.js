import init, { WmoEngine } from '../wasm/optimizer_wasm.js';

(async () => {
    try {
        await init();
        window.wasmEngine = new WmoEngine(BigInt(Date.now()));
        console.log('[WASM] optimizer engine ready');
    } catch (e) {
        console.warn('[WASM] failed to initialize, C# fallback will be used:', e);
    }
})();