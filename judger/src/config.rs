pub use crate::tester::exec::{Image, JudgerPrivateConfig, JudgerPublicConfig};
use serde::{self, Deserialize, Serialize};
use std::collections::HashMap;

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct JudgeToml {
    pub id: String,
    pub jobs: HashMap<String, JudgeTomlTestConfig>,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct JudgeTomlTestConfig {
    /// Base image to build from, if needed.
    pub image: Image,
    pub build: Option<Vec<String>>,
    pub run: Vec<String>,
}
