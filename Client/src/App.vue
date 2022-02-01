<template>
  <div class="w-full min-h-screen bg-gray-200">
    <div class="flex w-full justify-center text-white p-4 font-bold h-46">
      <div
        class="flex w-full md:w-1/2 lg:w-1/3 xl:w-1/4 justify-between text-base text-gray-800 lowercase"
      >
        <p class="cursor-pointer lato w-1/3" @click="page = 0">Home</p>
        <p>|</p>
        <p class="cursor-pointer lato w-1/3" @click="page = 1">ETF simulator</p>
        <p>|</p>
        <p class="cursor-pointer lato w-1/3" @click="page = 1">DCA portfolio</p>
      </div>
    </div>

    <div v-if="page == 0" class="w-full">
      <div class="mt-10 text-lg text-justify p-10 md:text-center">
        <p class="mb-4">Welcome!</p>
        <p class="mb-4">
          This site was made to generate various
          portfolio simulations based on past data.
        </p>
        <p class="mb-4">
          If you'd like to see the video this website was created for see below,
          if not just go to one of the pages in the navbar at the top to get
          started!
        </p>
        <p>
          Also, if you'd like to suggest a portfolio simulation, leave it as a
          comment on the video and if it's possible I'll add it!
        </p>
        <div class="w-2/3 mx-auto mt-10">
          <div class="p-bottom-56 overflow-hidden h-0 max-w-full relative">
            <iframe
              class="absolute inset-0 w-full h-full"
              src="https://www.youtube.com/embed/z9Ul9ccDOqE"
              frameborder="0"
            ></iframe>
          </div>
        </div>
      </div>
    </div>
    <ETF v-if="page == 1" />
  </div>
</template>

<script setup>
import { ref } from "vue";
import ETF from "./Views/ETF.vue";

const page = ref(0);
</script>

<style>
#app {
  font-family: "Barlow", sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

.lato {
  font-family: "Lato", sans-serif;
}

html,
body {
  margin: 0;
  padding: 0;
}

.p-bottom-56 {
  padding-bottom: 56.25%;
}
</style>
