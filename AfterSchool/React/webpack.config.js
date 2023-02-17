const path = require('path');
const WebpackNotifierPlugin = require('webpack-notifier');
const BrowserSyncPlugin = require('browser-sync-webpack-plugin');

module.exports = {
  entry: ['babel-polyfill', './src/index.jsx'],
  output: {
    path: path.resolve(__dirname, './dist'),
    filename: 'bundle.js',
  },
  module: {
    rules: [
      {
        test: /\.jsx$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
        },
      },
      {
        test: /\.(png|jpe?g|gif|svg)$/i,
        use: [
          {
            loader: 'url-loader',
            options: {
              limit: 8192,
            },
          },
        ],
      },
    ],
  },
  devtool: 'inline-source-map',
  plugins: [new WebpackNotifierPlugin(), new BrowserSyncPlugin()],
  resolve: {
    extensions: ['', '.js', '.jsx'],
  },
};
