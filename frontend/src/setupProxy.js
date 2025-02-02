const { createProxyMiddleware } = require("http-proxy-middleware");

module.exports = function (app) {
  app.use(
    "/api",
    createProxyMiddleware({
      target: "http://backend:8080", // Use service name and port
      changeOrigin: true,
      pathRewrite: { "^/api": "" }, // Adjust path if needed
    })
  );
};
