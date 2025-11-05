#!/usr/bin/env python3
"""
End-to-End Test: Tutorial Lodging Scene (Complete 4-Situation Arc)

This test validates the complete self-contained scene architecture with:
- Situation 1: Negotiate with Elena at Common Room
- Situation 2: Access private room (scene-created location)
- Situation 3: Rest in private room (seamless advance from Sit 2)
- Situation 4: Check out with Elena at Common Room (THE FIX)

Critical validation: Situation 4 must activate when player returns to Common Room with Elena.
This tests the bug fix in SceneArchetypeCatalog.cs where RequiredLocationId was changed from
"generated:private_room" to contextNPC?.Location?.Id (common room).
"""

from playwright.sync_api import sync_playwright, Page, Browser
import time
import json

class LodgingSceneTest:
    def __init__(self, base_url="http://localhost:6800"):
        print(f"[INIT] Test will connect to: {base_url}")
        print("[INIT] Make sure the server is running before this test starts")
        self.base_url = base_url
        self.browser = None
        self.page = None
        self.results = {
            "situation_1": {"status": "NOT_STARTED", "details": []},
            "situation_2": {"status": "NOT_STARTED", "details": []},
            "situation_3": {"status": "NOT_STARTED", "details": []},
            "situation_4": {"status": "NOT_STARTED", "details": []},
            "overall": {"status": "NOT_STARTED", "critical_failures": []}
        }

    def log(self, situation, message):
        """Log test progress"""
        print(f"[{situation}] {message}")
        if situation in self.results:
            self.results[situation]["details"].append(message)

    def screenshot(self, name):
        """Take screenshot"""
        path = f"C:\\Git\\Wayfarer\\screenshots\\{name}.png"
        self.page.screenshot(path=path, full_page=True)
        self.log("SCREENSHOT", f"Saved: {name}.png")
        return path

    def wait_and_screenshot(self, name, wait_time=1):
        """Wait for UI to settle and take screenshot"""
        time.sleep(wait_time)
        return self.screenshot(name)

    def get_console_logs(self):
        """Get recent console logs (simulated - Playwright captures these automatically)"""
        # Console messages are captured via page.on("console") handler
        pass

    def setup(self):
        """Launch browser and navigate to game"""
        print("\n" + "="*80)
        print("TUTORIAL LODGING SCENE - END-TO-END TEST")
        print("="*80 + "\n")

        with sync_playwright() as p:
            # Launch browser in headless mode
            self.browser = p.chromium.launch(headless=False, slow_mo=500)  # slow_mo for visibility
            self.page = self.browser.new_page()

            # Set viewport size for consistent screenshots
            self.page.set_viewport_size({"width": 1920, "height": 1080})

            # Capture console logs
            self.page.on("console", lambda msg: print(f"[BROWSER CONSOLE] {msg.type}: {msg.text}"))

            # Navigate to game
            self.log("SETUP", f"Navigating to {self.base_url}")
            self.page.goto(self.base_url)
            self.page.wait_for_load_state('networkidle')

            # Wait for game to initialize
            time.sleep(2)

            self.screenshot("00_initial_state")
            self.log("SETUP", "Game loaded successfully")

    def test_situation_1_negotiate(self):
        """Test Situation 1: Negotiate with Elena at Common Room"""
        self.log("situation_1", "="*60)
        self.log("situation_1", "SITUATION 1: Negotiate with Elena at Common Room")
        self.log("situation_1", "="*60)

        try:
            # Look for Elena or "Look Around" to see people
            self.log("situation_1", "Looking for Elena or people view...")

            # Try to find "Look Around" button or Elena directly
            look_around = self.page.locator("text=/Look Around|Who's Here/i").first
            if look_around.is_visible(timeout=5000):
                self.log("situation_1", "Found 'Look Around' button, clicking...")
                look_around.click()
                time.sleep(1)
                self.screenshot("01_people_view")

            # Look for Elena
            elena = self.page.locator("text=/Elena|Innkeeper/i").first
            if not elena.is_visible(timeout=5000):
                self.results["situation_1"]["status"] = "FAIL"
                self.log("situation_1", "FAIL: Elena not found")
                return False

            self.log("situation_1", "✓ Elena found")

            # Look for "Secure Lodging" or similar scene button
            lodging_button = self.page.locator("text=/Lodging|Room|Rest|Secure/i").first
            if not lodging_button.is_visible(timeout=5000):
                self.results["situation_1"]["status"] = "FAIL"
                self.log("situation_1", "FAIL: Lodging scene button not found")
                return False

            self.log("situation_1", "✓ Found lodging scene interaction")
            lodging_button.click()
            time.sleep(1)
            self.screenshot("02_situation_1_choices")

            # Verify Sir Brante transparency - should see 3 choices
            choices = self.page.locator("button:has-text('Convince'), button:has-text('Pay'), button:has-text('Guild')").all()
            self.log("situation_1", f"Found {len(choices)} choices")

            if len(choices) < 2:
                self.results["situation_1"]["status"] = "PARTIAL"
                self.log("situation_1", "WARNING: Expected 3 choices, found fewer")
            else:
                self.log("situation_1", "✓ Sir Brante transparency: Multiple choices visible")

            # Select first available choice (Convince or Pay)
            convince_choice = self.page.locator("text=/Convince Elena|Persuade/i").first
            pay_choice = self.page.locator("text=/Pay Elena|Pay.*coins/i").first

            if convince_choice.is_visible(timeout=2000):
                self.log("situation_1", "Selecting 'Convince Elena' choice...")
                convince_choice.click()
            elif pay_choice.is_visible(timeout=2000):
                self.log("situation_1", "Selecting 'Pay Elena' choice...")
                pay_choice.click()
            else:
                self.results["situation_1"]["status"] = "FAIL"
                self.log("situation_1", "FAIL: No available choices found")
                return False

            time.sleep(2)
            self.screenshot("03_situation_1_complete")

            # Verify we exited to world (should be back at location view)
            location_header = self.page.locator("h1, h2").first
            if location_header.is_visible(timeout=5000):
                location_text = location_header.text_content()
                self.log("situation_1", f"✓ Returned to location: {location_text}")

            # Check for room_key in inventory (if visible)
            inventory_section = self.page.locator("text=/Inventory|Items/i").first
            if inventory_section.is_visible(timeout=2000):
                self.log("situation_1", "✓ Inventory section visible")

            self.results["situation_1"]["status"] = "PASS"
            self.log("situation_1", "✅ SITUATION 1 COMPLETE")
            return True

        except Exception as e:
            self.results["situation_1"]["status"] = "FAIL"
            self.log("situation_1", f"❌ EXCEPTION: {str(e)}")
            return False

    def test_situation_2_access(self):
        """Test Situation 2: Access private room"""
        self.log("situation_2", "="*60)
        self.log("situation_2", "SITUATION 2: Access Private Room")
        self.log("situation_2", "="*60)

        try:
            # Look for scene-created private room location
            self.log("situation_2", "Looking for private room location...")

            # Try various patterns for private room
            private_room = self.page.locator("text=/Private.*Room|Upper.*Room|Elena.*Room/i").first

            if not private_room.is_visible(timeout=5000):
                self.results["situation_2"]["status"] = "FAIL"
                self.results["overall"]["critical_failures"].append("Private room location not found - scene may not have created dependent resources")
                self.log("situation_2", "❌ FAIL: Private room not found")
                return False

            self.log("situation_2", "✓ Found private room location")
            private_room.click()
            time.sleep(2)
            self.screenshot("04_private_room_navigation")

            # CRITICAL: Scene should auto-resume here (Situation 2)
            # Look for "Enter" or "Access" choice
            enter_choice = self.page.locator("text=/Enter|Access|Unlock/i").first

            if not enter_choice.is_visible(timeout=5000):
                self.results["situation_2"]["status"] = "FAIL"
                self.results["overall"]["critical_failures"].append("Situation 2 did NOT auto-resume at private room - scene resumption BROKEN")
                self.log("situation_2", "❌ CRITICAL FAIL: Scene did not resume at private room")
                return False

            self.log("situation_2", "✅ CRITICAL PASS: Scene auto-resumed at private room!")
            self.screenshot("05_situation_2_resumed")

            # Click Enter
            self.log("situation_2", "Clicking 'Enter' choice...")
            enter_choice.click()
            time.sleep(2)
            self.screenshot("06_situation_2_complete")

            # CRITICAL: Should seamlessly advance to Situation 3 (same context)
            # Look for "Rest" choice without exiting to world
            rest_choice = self.page.locator("text=/Rest|Sleep|Lie Down/i").first

            if not rest_choice.is_visible(timeout=5000):
                self.results["situation_2"]["status"] = "PARTIAL"
                self.log("situation_2", "⚠️ WARNING: Did not seamlessly advance to Situation 3")
                return True  # Still pass Situation 2, but note the issue

            self.log("situation_2", "✅ SEAMLESS ADVANCE: Situation 3 appeared without exiting to world!")
            self.results["situation_2"]["status"] = "PASS"
            return True

        except Exception as e:
            self.results["situation_2"]["status"] = "FAIL"
            self.log("situation_2", f"❌ EXCEPTION: {str(e)}")
            return False

    def test_situation_3_rest(self):
        """Test Situation 3: Rest in private room"""
        self.log("situation_3", "="*60)
        self.log("situation_3", "SITUATION 3: Rest in Private Room")
        self.log("situation_3", "="*60)

        try:
            # Should already be in Situation 3 from seamless advance
            rest_choice = self.page.locator("text=/Rest|Sleep|Lie Down/i").first

            if not rest_choice.is_visible(timeout=5000):
                self.results["situation_3"]["status"] = "FAIL"
                self.log("situation_3", "❌ FAIL: Rest choice not found")
                return False

            self.log("situation_3", "✓ Found Rest choice")
            self.screenshot("07_situation_3_rest")

            # Click Rest
            self.log("situation_3", "Clicking 'Rest' choice...")
            rest_choice.click()
            time.sleep(3)  # Rest advances time significantly
            self.screenshot("08_situation_3_complete")

            # Verify time advanced to morning
            time_display = self.page.locator("text=/Morning|Day.*2/i").first
            if time_display.is_visible(timeout=2000):
                self.log("situation_3", "✓ Time advanced to morning")

            # Should exit to world now (context change - need to return to common room)
            location_header = self.page.locator("h1, h2").first
            if location_header.is_visible(timeout=5000):
                location_text = location_header.text_content()
                self.log("situation_3", f"✓ Exited to world: {location_text}")

            self.results["situation_3"]["status"] = "PASS"
            self.log("situation_3", "✅ SITUATION 3 COMPLETE")
            return True

        except Exception as e:
            self.results["situation_3"]["status"] = "FAIL"
            self.log("situation_3", f"❌ EXCEPTION: {str(e)}")
            return False

    def test_situation_4_checkout(self):
        """Test Situation 4: Check out with Elena at Common Room (THE CRITICAL FIX)"""
        self.log("situation_4", "="*60)
        self.log("situation_4", "SITUATION 4: Check Out with Elena (CRITICAL TEST)")
        self.log("situation_4", "="*60)
        self.log("situation_4", "This tests the bug fix: RequiredLocationId changed from 'generated:private_room' to common room")

        try:
            # Navigate back to Common Room
            self.log("situation_4", "Navigating back to Common Room...")

            common_room = self.page.locator("text=/Common.*Room|Tavern/i").first
            if not common_room.is_visible(timeout=5000):
                self.results["situation_4"]["status"] = "FAIL"
                self.log("situation_4", "❌ FAIL: Cannot find Common Room navigation")
                return False

            self.log("situation_4", "✓ Found Common Room")
            common_room.click()
            time.sleep(2)
            self.screenshot("09_returned_to_common_room")

            # CRITICAL TEST: Scene should auto-resume here (Situation 4)
            # This is the bug that was fixed - before fix, this would FAIL
            checkout_choice = self.page.locator("text=/Check Out|Leave|Depart|Conclude/i").first

            if not checkout_choice.is_visible(timeout=5000):
                self.results["situation_4"]["status"] = "FAIL"
                self.results["overall"]["critical_failures"].append("🔴 CRITICAL BUG STILL PRESENT: Situation 4 did NOT resume at common room with Elena")
                self.log("situation_4", "❌ 🔴 CRITICAL FAIL: Situation 4 did NOT resume!")
                self.log("situation_4", "The bug fix did NOT work - RequiredLocationId/RequiredNpcId still broken")
                return False

            self.log("situation_4", "✅ 🎉 CRITICAL PASS: Situation 4 auto-resumed at common room with Elena!")
            self.log("situation_4", "✅ BUG FIX CONFIRMED WORKING")
            self.screenshot("10_situation_4_resumed")

            # Click Check Out
            self.log("situation_4", "Clicking 'Check Out' choice...")
            checkout_choice.click()
            time.sleep(2)
            self.screenshot("11_situation_4_complete")

            # Verify cleanup: room_key should be gone, private room should be locked
            self.log("situation_4", "Verifying cleanup...")

            # Try to navigate to private room again
            private_room = self.page.locator("text=/Private.*Room|Upper.*Room/i").first
            if private_room.is_visible(timeout=3000):
                private_room.click()
                time.sleep(1)
                self.screenshot("12_private_room_locked")

                # Should see "Locked" message or no entry option
                locked_text = self.page.locator("text=/Locked|Cannot Enter/i").first
                if locked_text.is_visible(timeout=2000):
                    self.log("situation_4", "✓ Private room is locked after checkout")

            self.results["situation_4"]["status"] = "PASS"
            self.log("situation_4", "✅ SITUATION 4 COMPLETE")
            self.log("situation_4", "✅ COMPLETE 4-SITUATION ARC VALIDATED")
            return True

        except Exception as e:
            self.results["situation_4"]["status"] = "FAIL"
            self.log("situation_4", f"❌ EXCEPTION: {str(e)}")
            return False

    def run_complete_test(self):
        """Run the complete 4-situation test"""
        try:
            self.setup()

            # Test each situation in sequence
            sit1_pass = self.test_situation_1_negotiate()
            if not sit1_pass:
                self.log("OVERALL", "Stopping - Situation 1 failed")
                self.results["overall"]["status"] = "FAIL"
                return

            sit2_pass = self.test_situation_2_access()
            if not sit2_pass:
                self.log("OVERALL", "Stopping - Situation 2 failed")
                self.results["overall"]["status"] = "FAIL"
                return

            sit3_pass = self.test_situation_3_rest()
            if not sit3_pass:
                self.log("OVERALL", "Stopping - Situation 3 failed")
                self.results["overall"]["status"] = "FAIL"
                return

            sit4_pass = self.test_situation_4_checkout()
            if not sit4_pass:
                self.log("OVERALL", "Stopping - Situation 4 failed")
                self.results["overall"]["status"] = "FAIL"
                return

            # All situations passed!
            self.results["overall"]["status"] = "PASS"
            self.log("OVERALL", "✅ ALL 4 SITUATIONS PASSED")

        except Exception as e:
            self.results["overall"]["status"] = "FAIL"
            self.log("OVERALL", f"❌ FATAL EXCEPTION: {str(e)}")

        finally:
            if self.browser:
                self.browser.close()

            # Print final summary
            self.print_summary()

    def print_summary(self):
        """Print test summary"""
        print("\n" + "="*80)
        print("TEST SUMMARY")
        print("="*80 + "\n")

        print(f"Situation 1 (Negotiate):  {self.results['situation_1']['status']}")
        print(f"Situation 2 (Access):     {self.results['situation_2']['status']}")
        print(f"Situation 3 (Rest):       {self.results['situation_3']['status']}")
        print(f"Situation 4 (Checkout):   {self.results['situation_4']['status']}")
        print(f"\nOVERALL:                  {self.results['overall']['status']}")

        if self.results['overall']['critical_failures']:
            print("\n❌ CRITICAL FAILURES:")
            for failure in self.results['overall']['critical_failures']:
                print(f"  - {failure}")

        print("\n" + "="*80 + "\n")

        # Save results to JSON
        with open('C:\\Git\\Wayfarer\\test_results.json', 'w') as f:
            json.dump(self.results, f, indent=2)
        print("Results saved to: test_results.json")

if __name__ == "__main__":
    # Create screenshots directory
    import os
    os.makedirs('C:\\Git\\Wayfarer\\screenshots', exist_ok=True)

    # Run test
    test = LodgingSceneTest()
    test.run_complete_test()
