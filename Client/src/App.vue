<template>
  <div class="w-screen h-screen bg-gray-800">
    <div class="flex w-full justify-end text-white p-4 font-bold">
      <div class="flex w-1/3 justify-around">
        <p class="cursor-pointer" @click="page = 0">Home</p>
        <p class="cursor-pointer" @click="page = 1">ETF simulator</p>
      </div>
    </div>

    <div v-if="page == 0" class="w-full">
      <p>Home</p>
    </div>
    <div class="flex justify-center" v-if="page == 1">
      <div class="m-6 w-full rounded bg-gray-200 flex justify-center flex-col items-center">
        <p class="p-6">
          This strategy simulates what dollar cost averaging into the top coins
          by market cap over time would be.
        </p>
        <div class="h-px w-10/12 bg-gray-800" />
        <p class="p-6">
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { inject, ref } from "vue";
import {NInput} from 'naive-ui'

const page = ref(0);
const http = inject("$http");

const etf = ref({
  amount: 100,
  coins: 100,
  interval: "monthly",
  start: "20180101",
  end: "20220131"
})

http
  .get("etf?amnt=100&coins=1&interval=monthly&start=20180101&end=20220131")
  .then((res) => console.log(res));
</script>

<style>
#app {
  font-family: "Barlow", sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

html,
body {
  margin: 0;
  padding: 0;
}
</style>
