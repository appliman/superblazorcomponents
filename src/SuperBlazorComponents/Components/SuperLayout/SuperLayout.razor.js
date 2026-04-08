export function isMobile() {
	// RegEx pour détecter les appareils mobiles
	const mobileRegex = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i;
	return mobileRegex.test(navigator.userAgent);
}

export function getDeviceInfo() {
	const userAgent = navigator.userAgent;
	const userAgentData = navigator.userAgentData;
	const screenWidth = screen.width;
	const screenHeight = screen.height;
	const availableWidth = screen.availWidth;
	const availableHeight = screen.availHeight;

	const windowInnerWidth = window.innerWidth;
	const windowInnerHeight = window.innerHeight;

	const mobileRegex = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i;
	let isMobile = mobileRegex.test(userAgent);
	let os = "Unknown OS";
	let platform = "Unkwown Platform";

	if (userAgentData) {
		platform = userAgentData.platform || platform;
		isMobile = userAgentData.isMobile;
	}

	if (/Win/i.test(userAgent)) {
		os = "Windows";
	} else if (/Mac/i.test(userAgent)) {
		os = "MacOS";
	} else if (/Linux/i.test(userAgent)) {
		os = "Linux";
	} else if (/Android/i.test(userAgent)) {
		os = "Android";
	} else if (/iOS|iPhone|iPad|iPod/i.test(userAgent)) {
		os = "iOS";
	}

	return {
		userAgent: userAgent,
		platform: platform,
		os: os,
		isMobile: isMobile,
		screenWidth: screenWidth,
		screenHeight: screenHeight,
		availableWidth: availableWidth,
		availableHeight: availableHeight,
		windowInnerWidth: windowInnerWidth,
		windowInnerHeight: windowInnerHeight
	};
}
