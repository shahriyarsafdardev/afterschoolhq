const path = require('path');
const Dotenv = require('dotenv-webpack');
const WebpackNotifierPlugin = require('webpack-notifier');
const BrowserSyncPlugin = require('browser-sync-webpack-plugin');

module.exports = {
  entry: ['babel-polyfill', './src/index.jsx'],
  output: {
    path: path.resolve(__dirname, './dist'),
    filename: 'bundle.js',
    chunkFilename: '[name].[contenthash].js'
  },
  module: {
    rules: [
      {
        test: /\.jsx$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader'
        }
      },
      {
        test: /\.(png|jpe?g|gif|svg)$/i,
        use: [
          {
            loader: 'url-loader',
            options: {
              limit: 8192
            }
          }
        ]
      }
    ]
  },
  plugins: [
    new WebpackNotifierPlugin(),
    new BrowserSyncPlugin(),
    new Dotenv({
      path: '.env'
    })
  ],
  resolve: {
    extensions: ['', '.js', '.jsx'],
    modules: [path.resolve('./src'), 'node_modules']
  }
};
